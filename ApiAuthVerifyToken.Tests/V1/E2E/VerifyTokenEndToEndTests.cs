using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using ApiAuthVerifyToken.Tests.V1.TestHelper;
using ApiAuthVerifyToken.V1.Boundary;
using ApiAuthVerifyToken.V1.Domain;
using ApiAuthVerifyToken.V1.Gateways;
using ApiAuthVerifyToken.V1.Infrastructure;
using ApiAuthVerifyToken.V1.UseCase;
using ApiAuthVerifyToken.V1.UseCase.Interfaces;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace ApiAuthVerifyToken.Tests.V1.AcceptanceTests
{
    [TestFixture]
    public class VerifyTokenEndToEndTests : DynamoDBTests
    {
        private VerifyTokenHandler _classUnderTest;
        private IAuthTokenDatabaseGateway _authTokenDatabaseGateway;
        private IServiceProvider _serviceProvider;
        private DynamoDBGateway _dynamoDbGateway;
        private string _jwtServiceFlow;
        private string _jwtUserFlow;
        private readonly Fixture _fixture = new Fixture();
        private readonly Faker _faker = new Faker();
        private List<string> _allowedGroups;
        
        [SetUp]
        public void Setup()
        {
            // Create real database connection
            var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};Port=5432;Database={Environment.GetEnvironmentVariable("DB_DATABASE")};Username={Environment.GetEnvironmentVariable("DB_USERNAME")};Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            // Initialize real DynamoDB gateway
            _dynamoDbGateway = new DynamoDBGateway(DynamoDbContext);
            
            // Initialize real AuthTokenDatabaseGateway
            var optionsBuilder = new DbContextOptionsBuilder<TokenDatabaseContext>();
            optionsBuilder.UseNpgsql(connection);
            using (var tokenDatabaseContext = new TokenDatabaseContext(optionsBuilder.Options))
            {
                _authTokenDatabaseGateway = new AuthTokenDatabaseGateway(tokenDatabaseContext);
            }
            
            // Set up environment variables
            Environment.SetEnvironmentVariable("jwtSecret", _faker.Random.AlphaNumeric(50));
            Environment.SetEnvironmentVariable("hackneyUserAuthTokenJwtSecret", _faker.Random.AlphaNumeric(50));
            
            // Set up JWT tokens
            _allowedGroups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            _jwtServiceFlow = GenerateJwtHelper.GenerateJwtToken();
            _jwtUserFlow = GenerateJwtHelper.GenerateJwtTokenUserFlow(_allowedGroups);
            
            // Setup real service provider
            var services = new ServiceCollection();
            services.AddSingleton<IAuthTokenDatabaseGateway>(_authTokenDatabaseGateway);
            services.AddSingleton<IDynamoDbGateway>(_dynamoDbGateway);
            services.AddSingleton<IVerifyAccessUseCase>(provider => 
                new VerifyAccessUseCase(
                    provider.GetRequiredService<IAuthTokenDatabaseGateway>(), 
                    provider.GetRequiredService<IDynamoDbGateway>()
                )
            );
            
            _serviceProvider = services.BuildServiceProvider();
            
            _classUnderTest = new VerifyTokenHandler(_serviceProvider);
        }

        [Test]
        public void ApiGatewayIdLookupShouldReturnAllowEffectWhenUserGroupsAreAllowed()
        {
            // Arrange
            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.Headers["Authorization"] = _jwtUserFlow;
            
            // Create and store API data in DynamoDB
            var apiData = _fixture.Build<APIDataUserFlowDbEntity>()
                .With(x => x.AllowedGroups, _allowedGroups)
                .With(x => x.Environment, lambdaRequest.RequestContext.Stage)
                .With(x => x.AwsAccount, lambdaRequest.RequestContext.AccountId)
                .With(x => x.ApiGatewayId, lambdaRequest.RequestContext.ApiId)
                .Create();
                
            AddDataToDynamoDb(apiData);

            // Act
            var result = _classUnderTest.VerifyToken(lambdaRequest);

            // Assert
            result.Should().BeOfType<APIGatewayCustomAuthorizerResponse>();
            result.PolicyDocument.Statement.First().Effect.Should().Be("Allow");
            result.PrincipalID.Should().NotBeNull();
        }
        
        [Test]
        public void ApiNameLookupShouldReturnDenyEffectWhenUserGroupsAreNotAllowed()
        {
            // Arrange
            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.Headers["Authorization"] = _jwtUserFlow;
            
            // Create and store API data with different groups in DynamoDB
            var nonAllowedGroups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            var apiName = _fixture.Create<string>();
            
            var apiData = _fixture.Build<APIDataUserFlowDbEntity>()
                .With(x => x.AllowedGroups, nonAllowedGroups)
                .With(x => x.Environment, lambdaRequest.RequestContext.Stage)
                .With(x => x.AwsAccount, lambdaRequest.RequestContext.AccountId)
                .With(x => x.ApiName, apiName)
                .Create();
                
            AddDataToDynamoDb(apiData);

            // Act
            var result = _classUnderTest.VerifyToken(lambdaRequest);

            // Assert
            result.Should().BeOfType<APIGatewayCustomAuthorizerResponse>();
            result.PolicyDocument.Statement.First().Effect.Should().Be("Deny");
        }
        
        [Test]
        public void ServiceFlowShouldReturnAllowEffectWhenTokenIsValid()
        {
            // Arrange
            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.Headers["Authorization"] = _jwtServiceFlow;
            
            var apiName = _fixture.Create<string>();
            var consumerName = _fixture.Create<string>();
            var tokenId = 123; // Use a specific ID to store in the real database
            
            // Create token data in the real database
            var tokenData = new AuthTokenServiceFlow
            {
                Id = tokenId,
                ApiEndpointName = lambdaRequest.RequestContext.Path,
                ApiName = apiName,
                Environment = lambdaRequest.RequestContext.Stage,
                HttpMethodType = lambdaRequest.RequestContext.HttpMethod,
                ConsumerName = consumerName,
                Enabled = true,
                ExpirationDate = null
            };
            
            // Store token data in the real database
            StoreTokenDataInDatabase(tokenData);

            // Act
            var result = _classUnderTest.VerifyToken(lambdaRequest);

            // Assert
            result.Should().BeOfType<APIGatewayCustomAuthorizerResponse>();
            result.PolicyDocument.Statement.First().Effect.Should().Be("Allow");
            result.PrincipalID.Should().Be(consumerName + tokenData.Id);
        }
        
        private void AddDataToDynamoDb(APIDataUserFlowDbEntity apiData)
        {
            Dictionary<string, AttributeValue> attributes = new Dictionary<string, AttributeValue>();
            attributes["apiName"] = new AttributeValue { S = apiData.ApiName };
            attributes["environment"] = new AttributeValue { S = apiData.Environment };
            attributes["awsAccount"] = new AttributeValue { S = apiData.AwsAccount };
            attributes["apiGatewayId"] = new AttributeValue { S = apiData.ApiGatewayId };
            attributes["allowedGroups"] = new AttributeValue { SS = new List<string>(apiData.AllowedGroups) };

            PutItemRequest request = new PutItemRequest
            {
                TableName = "APIAuthenticatorData",
                Item = attributes
            };

            DynamoDBClient.PutItemAsync(request).GetAwaiter().GetResult();
        }
        
        private void StoreTokenDataInDatabase(AuthTokenServiceFlow tokenData)
        {
            // Use the real gateway to store token data with raw SQL
            if (_authTokenDatabaseGateway is AuthTokenDatabaseGateway gateway)
            {
                var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};Port=5432;Database={Environment.GetEnvironmentVariable("DB_DATABASE")};Username={Environment.GetEnvironmentVariable("DB_USERNAME")};Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();
                
                var sql = @"INSERT INTO auth_tokens 
                            (id, api_endpoint_name, api_name, environment, http_method_type, consumer_name, enabled, expiration_date) 
                            VALUES 
                            (@id, @apiEndpoint, @apiName, @environment, @httpMethod, @consumer, @enabled, @expiration)";
                
                using var cmd = new NpgsqlCommand(sql, connection);
                
                cmd.Parameters.AddWithValue("@id", tokenData.Id);
                cmd.Parameters.AddWithValue("@apiEndpoint", tokenData.ApiEndpointName);
                cmd.Parameters.AddWithValue("@apiName", tokenData.ApiName);
                cmd.Parameters.AddWithValue("@environment", tokenData.Environment);
                cmd.Parameters.AddWithValue("@httpMethod", tokenData.HttpMethodType);
                cmd.Parameters.AddWithValue("@consumer", tokenData.ConsumerName);
                cmd.Parameters.AddWithValue("@enabled", tokenData.Enabled);
                cmd.Parameters.AddWithValue("@expiration", tokenData.ExpirationDate ?? (object)DBNull.Value);
                
                cmd.ExecuteNonQuery();
            }
        }
    }
}