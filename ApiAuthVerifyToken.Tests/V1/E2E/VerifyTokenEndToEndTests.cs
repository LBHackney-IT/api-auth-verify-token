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
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.DataModel;

namespace ApiAuthVerifyToken.Tests.V1.E2E
{
    [TestFixture]
    public class VerifyTokenEndToEndTests : DatabaseTests
    {
        private VerifyTokenHandler _classUnderTest;
        private DynamoDBGateway _dynamoDbGateway;
        private string _jwtUserFlow;
        private readonly Fixture _fixture = new Fixture();
        private readonly Faker _faker = new Faker();
        private List<string> _allowedGroups;
        protected DynamoDBContext DynamoDbContext { get; private set; }
        protected AmazonDynamoDBClient DynamoDBClient { get; private set; }

        [SetUp]
        public void Setup()
        {
            SetupDynamoDb();
            _dynamoDbGateway = new DynamoDBGateway(DynamoDbContext);
            Environment.SetEnvironmentVariable("jwtSecret", _faker.Random.AlphaNumeric(50));
            Environment.SetEnvironmentVariable("hackneyUserAuthTokenJwtSecret", _faker.Random.AlphaNumeric(50));
            _allowedGroups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            _jwtUserFlow = GenerateJwtHelper.GenerateJwtTokenUserFlow(_allowedGroups);
            _classUnderTest = new VerifyTokenHandler(new ServiceCollection()
                .AddSingleton<IAuthTokenDatabaseGateway>(new AuthTokenDatabaseGateway(DatabaseContext))
                .AddSingleton<IDynamoDbGateway>(_dynamoDbGateway)
                .AddSingleton<IVerifyAccessUseCase, VerifyAccessUseCase>()
                .BuildServiceProvider());
            ClearDynamoDbTable();
            TruncateAllTables();
        }

        [TearDown]
        public void TearDown()
        {
            ClearDynamoDbTable();
            DynamoDBClient?.Dispose();
            TruncateAllTables();
        }

        [Test]
        public void ApiGatewayIdLookupShouldReturnAllowEffectWhenUserGroupsAreAllowed()
        {
            // Arrange
            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.Headers["Authorization"] = _jwtUserFlow;
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
            result.PolicyDocument.Statement.First().Effect.Should().Be("Allow");
        }

        [Test]
        public void ApiNameLookupShouldReturnDenyEffectWhenUserGroupsAreNotAllowed()
        {
            // Arrange
            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.Headers["Authorization"] = _jwtUserFlow;
            var nonAllowedGroups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            var apiName = _fixture.Create<string>();
            var apiData = _fixture.Build<APIDataUserFlowDbEntity>()
                .With(x => x.AllowedGroups, nonAllowedGroups)
                .With(x => x.Environment, lambdaRequest.RequestContext.Stage)
                .With(x => x.AwsAccount, lambdaRequest.RequestContext.AccountId)
                .With(x => x.ApiName, apiName)
                .With(x => x.ApiGatewayId, lambdaRequest.RequestContext.ApiId)
                .Create();
            AddDataToDynamoDb(apiData);

            // Act
            var result = _classUnderTest.VerifyToken(lambdaRequest);

            // Assert
            result.PolicyDocument.Statement.First().Effect.Should().Be("Deny");
        }

        [Test]
        public void ServiceFlowShouldReturnAllowEffectWhenTokenIsValid()
        {
            // Arrange
            var tokenId = 1;
            var token = GenerateJwtHelper.GenerateJwtToken(id: tokenId);
            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.RequestContext.HttpMethod = "GET";
            lambdaRequest.Headers["Authorization"] = token;
            var apiName = _fixture.Create<string>();
            var tokenData = new AuthTokenServiceFlow
            {
                Id = _fixture.Create<int>(),
                ApiEndpointName = lambdaRequest.RequestContext.Path,
                ApiName = apiName,
                Environment = lambdaRequest.RequestContext.Stage,
                HttpMethodType = lambdaRequest.RequestContext.HttpMethod,
                ConsumerName = _fixture.Create<string>(),
                ConsumerType = _fixture.Create<string>(),
                Enabled = true,
                ExpirationDate = null
            };
            StoreTokenDataInDatabase(tokenData);
            var apiData = _fixture.Build<APIDataUserFlowDbEntity>()
                .With(x => x.ApiName, apiName)
                .With(x => x.Environment, lambdaRequest.RequestContext.Stage)
                .With(x => x.AwsAccount, lambdaRequest.RequestContext.AccountId)
                .With(x => x.ApiGatewayId, lambdaRequest.RequestContext.ApiId)
                .With(x => x.AllowedGroups, new List<string> { _faker.Random.Word(), _faker.Random.Word() })
                .Create();
            AddDataToDynamoDb(apiData);
            DatabaseContext.SaveChanges();

            // Act
            var result = _classUnderTest.VerifyToken(lambdaRequest);

            // Assert
            result.PolicyDocument.Statement.First().Effect.Should().Be("Allow");
            result.PrincipalID.Should().Be(tokenData.ConsumerName + tokenId);
        }

        [Test]
        public void ServiceFlowShouldReturnDenyEffectWhenTokenIsInvalid()
        {
            // Arrange
            var tokenId = 1;
            var token = GenerateJwtHelper.GenerateJwtToken(id: tokenId);
            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.RequestContext.HttpMethod = "GET";
            lambdaRequest.Headers["Authorization"] = token;
            var apiName = _fixture.Create<string>();
            var tokenData = new AuthTokenServiceFlow
            {
                Id = _fixture.Create<int>(),
                ApiEndpointName = lambdaRequest.RequestContext.Path,
                ApiName = apiName,
                Environment = lambdaRequest.RequestContext.Stage,
                HttpMethodType = lambdaRequest.RequestContext.HttpMethod,
                ConsumerName = _faker.Random.Word(),
                ConsumerType = _faker.Random.Word(),
                Enabled = true,
                ExpirationDate = null
            };
            StoreTokenDataInDatabase(tokenData);
            var apiData = _fixture.Build<APIDataUserFlowDbEntity>()
                .With(x => x.ApiName, apiName)
                .With(x => x.Environment, lambdaRequest.RequestContext.Stage)
                .With(x => x.AwsAccount, lambdaRequest.RequestContext.AccountId)
                .With(x => x.ApiGatewayId, lambdaRequest.RequestContext.ApiId)
                .With(x => x.AllowedGroups, new List<string> { _faker.Random.Word(), _faker.Random.Word() })
                .Create();
            AddDataToDynamoDb(apiData);
            DatabaseContext.SaveChanges();

            // Invalidate the token by changing the secret
            Environment.SetEnvironmentVariable("jwtSecret", _faker.Random.AlphaNumeric(50));

            // Act
            var result = _classUnderTest.VerifyToken(lambdaRequest);

            // Assert
            result.PolicyDocument.Statement.First().Effect.Should().Be("Deny");
        }

        private void SetupDynamoDb()
        {
            var clientConfig = new AmazonDynamoDBConfig { ServiceURL = Environment.GetEnvironmentVariable("DynamoDb_LocalServiceUrl") };
            DynamoDBClient = new AmazonDynamoDBClient(clientConfig);
            var tableAlreadyExists = DynamoDBClient.ListTablesAsync().GetAwaiter().GetResult().TableNames
                .Any(x => x.Equals("APIAuthenticatorData", StringComparison.OrdinalIgnoreCase));
            if (!tableAlreadyExists)
            {
                var request = new CreateTableRequest
                {
                    TableName = "APIAuthenticatorData",
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition { AttributeName = "apiName", AttributeType = ScalarAttributeType.S },
                        new AttributeDefinition { AttributeName = "environment", AttributeType = ScalarAttributeType.S },
                        new AttributeDefinition { AttributeName = "apiGatewayId", AttributeType = ScalarAttributeType.S }
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement { AttributeName = "apiName", KeyType = KeyType.HASH },
                        new KeySchemaElement { AttributeName = "environment", KeyType = KeyType.RANGE }
                    },
                    GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                    {
                        new GlobalSecondaryIndex
                        {
                            IndexName = "apiGatewayIdIndex",
                            KeySchema = new List<KeySchemaElement>
                            {
                                new KeySchemaElement { AttributeName = "apiGatewayId", KeyType = KeyType.HASH }
                            },
                            Projection = new Projection { ProjectionType = ProjectionType.ALL },
                        }
                    },
                    BillingMode = BillingMode.PAY_PER_REQUEST
                };
                DynamoDBClient.CreateTableAsync(request).GetAwaiter().GetResult();
                var tableStatus = string.Empty;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    try
                    {
                        var describeResult = DynamoDBClient.DescribeTableAsync(new DescribeTableRequest { TableName = "APIAuthenticatorData" }).GetAwaiter().GetResult();
                        tableStatus = describeResult.Table.TableStatus;
                    }
                    catch (ResourceNotFoundException) { }
                } while (tableStatus != TableStatus.ACTIVE);
            }
            DynamoDbContext = new DynamoDBContext(DynamoDBClient);
        }

        private void AddDataToDynamoDb(APIDataUserFlowDbEntity apiData)
        {
            var attributes = new Dictionary<string, AttributeValue>
            {
                ["apiName"] = new AttributeValue { S = apiData.ApiName },
                ["environment"] = new AttributeValue { S = apiData.Environment },
                ["awsAccount"] = new AttributeValue { S = apiData.AwsAccount },
                ["apiGatewayId"] = new AttributeValue { S = apiData.ApiGatewayId },
                ["allowedGroups"] = (apiData.AllowedGroups != null && apiData.AllowedGroups.Any()) ? new AttributeValue { SS = new List<string>(apiData.AllowedGroups) } : new AttributeValue { NULL = true }
            };
            DynamoDBClient.PutItemAsync(new PutItemRequest { TableName = "APIAuthenticatorData", Item = attributes }).GetAwaiter().GetResult();
        }

        private void ClearDynamoDbTable()
        {
            var scanResponse = DynamoDBClient.ScanAsync(new ScanRequest { TableName = "APIAuthenticatorData" }).GetAwaiter().GetResult();
            foreach (var item in scanResponse.Items)
            {
                DynamoDBClient.DeleteItemAsync(new DeleteItemRequest
                {
                    TableName = "APIAuthenticatorData",
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "apiName", item["apiName"] },
                        { "environment", item["environment"] }
                    }
                }).GetAwaiter().GetResult();
            }
        }
    }
}
