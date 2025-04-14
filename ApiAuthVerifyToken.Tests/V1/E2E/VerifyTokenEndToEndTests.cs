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

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities

namespace ApiAuthVerifyToken.Tests.V1.E2E
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
            using var connection = new NpgsqlConnection(ConnectionString.TestDatabase());
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

            ClearDynamoDbTable();
            TruncateAllTables(connection);
        }

        [TearDown]
        public void TearDown()
        {
            ClearDynamoDbTable();

            using var connection = new NpgsqlConnection(ConnectionString.TestDatabase());
            connection.Open();
            TruncateAllTables(connection);
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

            var decoded = GenerateJwtHelper.DecodeJwtToken(lambdaRequest.Headers["Authorization"]);
            var payload = decoded.Payload;
            var tokenId = int.Parse(payload["id"].ToString(), System.Globalization.CultureInfo.InvariantCulture);

            var apiName = _fixture.Create<string>();
            var consumerName = _fixture.Create<string>();

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

            // Create and store API data in DynamoDB
            var apiData = _fixture.Build<APIDataUserFlowDbEntity>()
                .With(x => x.ApiName, apiName)
                .With(x => x.Environment, lambdaRequest.RequestContext.Stage)
                .With(x => x.AwsAccount, lambdaRequest.RequestContext.AccountId)
                .With(x => x.ApiGatewayId, lambdaRequest.RequestContext.ApiId)
                .With(x => x.AllowedGroups, new List<string> { consumerName })
                .Create();

            AddDataToDynamoDb(apiData);

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

            // check the item was added
            var getItemRequest = new GetItemRequest
            {
                TableName = "APIAuthenticatorData",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "apiName", new AttributeValue { S = apiData.ApiName } },
                    { "environment", new AttributeValue { S = apiData.Environment } }
                }
            };
            var getItemResponse = DynamoDBClient.GetItemAsync(getItemRequest).GetAwaiter().GetResult();
            getItemResponse.Item.Should().NotBeNull();
            getItemResponse.Item["apiName"].S.Should().Be(apiData.ApiName);
        }

        private void ClearDynamoDbTable()
        {
            var scanRequest = new ScanRequest
            {
                TableName = "APIAuthenticatorData"
            };

            var scanResponse = DynamoDBClient.ScanAsync(scanRequest).GetAwaiter().GetResult();
            foreach (var item in scanResponse.Items)
            {
                var deleteItemRequest = new DeleteItemRequest
                {
                    TableName = "APIAuthenticatorData",
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "apiName", item["apiName"] },
                        { "environment", item["environment"] }
                    }
                };
                DynamoDBClient.DeleteItemAsync(deleteItemRequest).GetAwaiter().GetResult();
            }
        }

        private static void StoreTokenDataInDatabase(AuthTokenServiceFlow tokenData)
        {
            // Use the real gateway to store token data with raw SQL
            using var connection = new NpgsqlConnection(ConnectionString.TestDatabase());
            connection.Open();

            // Insert into api_lookup table
            var apiLookupSql = @"INSERT INTO api_lookup (api_name, api_gateway_id) 
                         VALUES (@apiName, @apiGatewayId) 
                         RETURNING id";
            using var apiLookupCmd = new NpgsqlCommand(apiLookupSql, connection);
            apiLookupCmd.Parameters.AddWithValue("@apiName", ValidateInput(tokenData.ApiName));
            apiLookupCmd.Parameters.AddWithValue("@apiGatewayId", ValidateInput(tokenData.ApiName + "_id"));
            var apiLookupId = (int) apiLookupCmd.ExecuteScalar();

            // Insert into api_endpoint_lookup table
            var apiEndpointLookupSql = @"INSERT INTO api_endpoint_lookup (endpoint_name, api_lookup_id) 
                             VALUES (@endpointName, @apiLookupId) 
                             RETURNING id";
            using var apiEndpointLookupCmd = new NpgsqlCommand(apiEndpointLookupSql, connection);
            apiEndpointLookupCmd.Parameters.AddWithValue("@endpointName", tokenData.ApiEndpointName);
            apiEndpointLookupCmd.Parameters.AddWithValue("@apiLookupId", apiLookupId);
            var apiEndpointLookupId = (int) apiEndpointLookupCmd.ExecuteScalar();

            // Ensure consumer_type_lookup table has a unique constraint on consumer_name
            var ensureConstraintSql = @"DO $$
                                BEGIN
                                    IF NOT EXISTS (
                                        SELECT 1
                                        FROM information_schema.table_constraints
                                        WHERE constraint_name = 'consumer_name_unique'
                                    ) THEN
                                        ALTER TABLE consumer_type_lookup 
                                        ADD CONSTRAINT consumer_name_unique UNIQUE (consumer_name);
                                    END IF;
                                END $$;";
            using var ensureConstraintCmd = new NpgsqlCommand(ensureConstraintSql, connection);
            ensureConstraintCmd.ExecuteNonQuery();

            // Insert into consumer_type_lookup table
            var consumerTypeLookupSql = @"INSERT INTO consumer_type_lookup (consumer_name) 
                                  VALUES (@consumerName) 
                                  ON CONFLICT (consumer_name) DO NOTHING 
                                  RETURNING id";
            using var consumerTypeLookupCmd = new NpgsqlCommand(consumerTypeLookupSql, connection);
            consumerTypeLookupCmd.Parameters.AddWithValue("@consumerName", tokenData.ConsumerName);
            var consumerTypeLookupId = consumerTypeLookupCmd.ExecuteScalar() as int? ?? GetConsumerTypeId(connection, tokenData.ConsumerName);

            // Insert into tokens table
            var tokensSql = @"INSERT INTO tokens 
                      (api_lookup_id, api_endpoint_lookup_id, http_method_type, environment, consumer_name, consumer_type_lookup, requested_by, authorized_by, date_created, expiration_date, enabled) 
                      VALUES 
                      (@apiLookupId, @apiEndpointLookupId, @httpMethodType, @environment, @consumerName, @consumerTypeLookup, @requestedBy, @authorizedBy, @dateCreated, @expirationDate, @enabled)";
            using var tokensCmd = new NpgsqlCommand(tokensSql, connection);
            tokensCmd.Parameters.AddWithValue("@apiLookupId", apiLookupId);
            tokensCmd.Parameters.AddWithValue("@apiEndpointLookupId", apiEndpointLookupId);
            tokensCmd.Parameters.AddWithValue("@httpMethodType", ValidateInput(tokenData.HttpMethodType));
            tokensCmd.Parameters.AddWithValue("@environment", ValidateInput(tokenData.Environment));
            tokensCmd.Parameters.AddWithValue("@consumerName", tokenData.ConsumerName);
            tokensCmd.Parameters.AddWithValue("@consumerTypeLookup", consumerTypeLookupId);
            tokensCmd.Parameters.AddWithValue("@requestedBy", "test");
            tokensCmd.Parameters.AddWithValue("@authorizedBy", "test");
            tokensCmd.Parameters.AddWithValue("@dateCreated", DateTime.UtcNow);
            tokensCmd.Parameters.AddWithValue("@expirationDate", tokenData.ExpirationDate ?? (object) DBNull.Value);
            tokensCmd.Parameters.AddWithValue("@enabled", tokenData.Enabled);
            tokensCmd.ExecuteNonQuery();

            // Ensure token is valid for the test
            if (!tokenData.Enabled)
            {
                throw new InvalidOperationException("Token must be enabled for validation.");
            }
        }

        private static string ValidateInput(string input, int maxLength = 6)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be null or empty.");
            return input.Length > maxLength ? input.Substring(0, maxLength) : input.Trim();
        }

        private static int GetConsumerTypeId(NpgsqlConnection connection, string consumerName)
        {
            var sql = @"SELECT id FROM consumer_type_lookup WHERE consumer_name = @consumerName";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@consumerName", consumerName);
            return (int) cmd.ExecuteScalar();
        }

        private static void TruncateAllTables(NpgsqlConnection connection)
        {
            var truncateSql = @"TRUNCATE TABLE api_lookup, api_endpoint_lookup, consumer_type_lookup, tokens CASCADE";
            using var cmd = new NpgsqlCommand(truncateSql, connection);
            cmd.ExecuteNonQuery();
        }
    }
}
