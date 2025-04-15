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
using Amazon.DynamoDBv2.DataModel;

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities

namespace ApiAuthVerifyToken.Tests.V1.E2E
{
    [TestFixture]
    public class VerifyTokenEndToEndTests : DatabaseTests
    {
        private VerifyTokenHandler _classUnderTest;
        private IAuthTokenDatabaseGateway _authTokenDatabaseGateway;
        private IServiceProvider _serviceProvider;
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

            _authTokenDatabaseGateway = new AuthTokenDatabaseGateway(DatabaseContext);
            // Removed initialization of _dbConnectionForTest

            Environment.SetEnvironmentVariable("jwtSecret", _faker.Random.AlphaNumeric(50));
            Environment.SetEnvironmentVariable("hackneyUserAuthTokenJwtSecret", _faker.Random.AlphaNumeric(50));

            _allowedGroups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            _jwtUserFlow = GenerateJwtHelper.GenerateJwtTokenUserFlow(_allowedGroups);

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
            TruncateAllTables();
        }

        [TearDown]
        public void TearDown()
        {
            ClearDynamoDbTable();
            DynamoDBClient?.Dispose(); // Use null-conditional operator

            // TruncateAllTables();

            // Base TearDown should handle context disposal if context is IDisposable
            // (_serviceProvider as IDisposable)?.Dispose(); // Dispose DI container if necessary
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
                .With(x => x.ApiGatewayId, lambdaRequest.RequestContext.ApiId)
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
            var tokenId = 1;
            var token = GenerateJwtHelper.GenerateJwtToken(id: tokenId);
            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.RequestContext.HttpMethod = "GET";
            lambdaRequest.Headers["Authorization"] = token;

            var apiName = _fixture.Create<string>();

            // Create token data in the real database
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

            // Store token data in the real database USING THE CONTEXT'S CONNECTION/TRANSACTION
            StoreTokenDataInDatabase(DatabaseContext, tokenData); // Pass the context

            // Create and store API data in DynamoDB
            var apiData = _fixture.Build<APIDataUserFlowDbEntity>()
                .With(x => x.ApiName, apiName)
                .With(x => x.Environment, lambdaRequest.RequestContext.Stage)
                .With(x => x.AwsAccount, lambdaRequest.RequestContext.AccountId)
                .With(x => x.ApiGatewayId, lambdaRequest.RequestContext.ApiId)
                .With(x => x.AllowedGroups, new List<string> { _faker.Random.Word(), _faker.Random.Word() })
                .Create();

            AddDataToDynamoDb(apiData);
            DatabaseContext.SaveChanges(); // Save changes to the database

            // Act
            var result = _classUnderTest.VerifyToken(lambdaRequest);

            // Assert
            result.Should().BeOfType<APIGatewayCustomAuthorizerResponse>(); // Re-enable assertions
            result.PolicyDocument.Statement.First().Effect.Should().Be("Allow");
            result.PrincipalID.Should().Be(tokenData.ConsumerName + tokenId);
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
                            Projection = new Projection
                            {
                                ProjectionType = ProjectionType.ALL
                            },
                        }
                    },
                    BillingMode = BillingMode.PAY_PER_REQUEST
                };

                DynamoDBClient.CreateTableAsync(request).GetAwaiter().GetResult();

                // Wait for table to become active (important for tests)
                var tableStatus = string.Empty;
                do
                {
                    System.Threading.Thread.Sleep(1000); // Wait 1 second
                    try
                    {
                        var describeResult = DynamoDBClient.DescribeTableAsync(new DescribeTableRequest { TableName = "APIAuthenticatorData" }).GetAwaiter().GetResult();
                        tableStatus = describeResult.Table.TableStatus;
                    }
                    catch (ResourceNotFoundException)
                    {
                        // Table might not be immediately visible after creation
                    }
                } while (tableStatus != TableStatus.ACTIVE);

            }
            DynamoDbContext = new DynamoDBContext(DynamoDBClient);
        }

        private void AddDataToDynamoDb(APIDataUserFlowDbEntity apiData)
        {
            Dictionary<string, AttributeValue> attributes = new Dictionary<string, AttributeValue>();
            attributes["apiName"] = new AttributeValue { S = apiData.ApiName };
            attributes["environment"] = new AttributeValue { S = apiData.Environment };
            attributes["awsAccount"] = new AttributeValue { S = apiData.AwsAccount };
            attributes["apiGatewayId"] = new AttributeValue { S = apiData.ApiGatewayId };
            // Ensure AllowedGroups is not null or empty before adding
            if (apiData.AllowedGroups != null && apiData.AllowedGroups.Any())
            {
                attributes["allowedGroups"] = new AttributeValue { SS = new List<string>(apiData.AllowedGroups) };
            }
            else
            {
                // Handle case where AllowedGroups is empty or null if necessary,
                // e.g., don't add the attribute or add an empty list based on DynamoDB schema/logic
                attributes["allowedGroups"] = new AttributeValue { NULL = true }; // Or omit the attribute entirely
            }


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
                        // Use the actual primary key schema defined for the table
                        { "apiName", item["apiName"] },
                        { "environment", item["environment"] }
                    }
                };
                DynamoDBClient.DeleteItemAsync(deleteItemRequest).GetAwaiter().GetResult();
            }
        }

        // Modify StoreTokenDataInDatabase to accept the context
        private static void StoreTokenDataInDatabase(TokenDatabaseContext context, AuthTokenServiceFlow tokenData)
        {
            // Use the context's existing connection and transaction
            var connection = context.Database.GetDbConnection() as NpgsqlConnection;

            // Insert into api_lookup table
            var apiLookupSql = @"INSERT INTO api_lookup (api_name)
                         VALUES (@apiName)
                         RETURNING id";
            // Removed api_gateway_id as it wasn't in the original AuthTokens entity or used later
            using var apiLookupCmd = new NpgsqlCommand(apiLookupSql, connection); // Assign transaction
            apiLookupCmd.Parameters.AddWithValue("@apiName", ValidateInput(tokenData.ApiName));
            // Removed setting api_gateway_id parameter
            var apiLookupId = (int) apiLookupCmd.ExecuteScalar(); // This should no longer hang

            // Insert into api_endpoint_lookup table
            var apiEndpointLookupSql = @"INSERT INTO api_endpoint_lookup (endpoint_name, api_lookup_id)
                             VALUES (@endpointName, @apiLookupId)
                             RETURNING id";
            using var apiEndpointLookupCmd = new NpgsqlCommand(apiEndpointLookupSql, connection); // Assign transaction
            apiEndpointLookupCmd.Parameters.AddWithValue("@endpointName", ValidateInput(tokenData.ApiEndpointName)); // Validate endpoint name too
            apiEndpointLookupCmd.Parameters.AddWithValue("@apiLookupId", apiLookupId);
            var apiEndpointLookupId = (int) apiEndpointLookupCmd.ExecuteScalar();

            // Insert into consumer_type_lookup table (using ON CONFLICT DO NOTHING)
            var consumerTypeLookupSql = @"INSERT INTO consumer_type_lookup (consumer_name)
                                  VALUES (@consumerName)
                                  ON CONFLICT DO NOTHING
                                  RETURNING id";

            int consumerTypeLookupId;
            using (var consumerTypeLookupCmd = new NpgsqlCommand(consumerTypeLookupSql, connection))
            {
                consumerTypeLookupCmd.Parameters.AddWithValue("@consumerName", ValidateInput(tokenData.ConsumerType));
                var result = consumerTypeLookupCmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    // Query the ID separately if the row already exists
                    var getIdSql = "SELECT id FROM consumer_type_lookup WHERE consumer_name = @consumerName";
                    using (var getIdCmd = new NpgsqlCommand(getIdSql, connection))
                    {
                        getIdCmd.Parameters.AddWithValue("@consumerName", ValidateInput(tokenData.ConsumerType));
                        result = getIdCmd.ExecuteScalar();

                        if (result == null || result == DBNull.Value)
                        {
                            throw new InvalidOperationException($"Could not find consumer type '{tokenData.ConsumerType}' after insert attempt.");
                        }
                        consumerTypeLookupId = (int) result;
                    }
                }
                else
                {
                    consumerTypeLookupId = (int) result;
                }
            }

            var tokensSql = @"INSERT INTO tokens
                      (api_lookup_id, api_endpoint_lookup_id, http_method_type, environment, consumer_name, consumer_type_lookup, requested_by, authorized_by, date_created, expiration_date, enabled)
                      VALUES
                      (@apiLookupId, @apiEndpointLookupId, @httpMethodType, @environment, @consumerName, @consumerTypeLookupId, @requestedBy, @authorizedBy, @dateCreated, @expirationDate, @enabled)
                      RETURNING id"; // Return the generated token ID
            using var tokensCmd = new NpgsqlCommand(tokensSql, connection); // Assign transaction
            tokensCmd.Parameters.AddWithValue("@apiLookupId", apiLookupId);
            tokensCmd.Parameters.AddWithValue("@apiEndpointLookupId", apiEndpointLookupId);
            tokensCmd.Parameters.AddWithValue("@httpMethodType", ValidateInput(tokenData.HttpMethodType, 6));
            tokensCmd.Parameters.AddWithValue("@environment", ValidateInput(tokenData.Environment, 255));
            tokensCmd.Parameters.AddWithValue("@consumerName", ValidateInput(tokenData.ConsumerName, 255));
            tokensCmd.Parameters.AddWithValue("@consumerTypeLookupId", consumerTypeLookupId);
            tokensCmd.Parameters.AddWithValue("@requestedBy", ValidateInput("E2ETest", 255));
            tokensCmd.Parameters.AddWithValue("@authorizedBy", ValidateInput("E2ETest", 255));
            tokensCmd.Parameters.AddWithValue("@dateCreated", DateTime.UtcNow);
            tokensCmd.Parameters.AddWithValue("@expirationDate", tokenData.ExpirationDate.HasValue ? (object) tokenData.ExpirationDate.Value : DBNull.Value);
            tokensCmd.Parameters.AddWithValue("@enabled", tokenData.Enabled);

            // Update the tokenData object with the real ID generated by the database
            // We need this if the test logic relies on the actual ID.
            var generatedTokenId = (int) tokensCmd.ExecuteScalar();
            tokenData.Id = generatedTokenId; // Update the object


            // Do NOT call SaveChanges here - the transaction will be rolled back in TearDown by the base class
            // Do NOT close the connection here - it's managed by the context/test runner/base class
        }

        // Helper to get ConsumerTypeId by Name if ON CONFLICT DO NOTHING is used (or for verification)
        private static int GetConsumerTypeIdByName(NpgsqlConnection connection, NpgsqlTransaction transaction, string consumerTypeName)
        {
            // Adjust column name if needed (e.g., type_name)
            var sql = @"SELECT id FROM consumer_type_lookup WHERE type_name = @typeName";
            using var cmd = new NpgsqlCommand(sql, connection, transaction); // Use existing transaction
            cmd.Parameters.AddWithValue("@typeName", consumerTypeName);
            var result = cmd.ExecuteScalar();
            if (result == null || result == DBNull.Value)
            {
                throw new InvalidOperationException($"Consumer type '{consumerTypeName}' not found after insert/conflict.");
            }
            return (int) result;
        }

        // Removed GetConsumerTypeId method

        // Ensure ValidateInput exists or add it back if needed
        private static string ValidateInput(string input, int maxLength = 50) // Increased default length slightly
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            // Allow empty strings? Depending on requirements.
            // If whitespace should be treated as empty:
            // if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            input = input.Trim();
            return input.Length > maxLength ? input.Substring(0, maxLength) : input;
        }


        private void TruncateAllTables()
        {
            if (DatabaseContext.Database.GetDbConnection() is not NpgsqlConnection connection || connection.State != System.Data.ConnectionState.Open)
            {
                // Log or handle error: Cannot truncate if connection is not open or valid.
                Console.WriteLine("Warning: Could not truncate tables, connection not open or invalid.");
                return;
            }
            // Ensure table names match your schema exactly (case-sensitive on some systems)
            var truncateSql = @"TRUNCATE TABLE public.api_lookup, public.api_endpoint_lookup, public.consumer_type_lookup, public.tokens RESTART IDENTITY CASCADE";
            using var cmd = connection.CreateCommand();
            cmd.CommandText = truncateSql;
            cmd.ExecuteNonQuery();

            // check the tables are empty
            var checkSql = @"SELECT COUNT(*) FROM public.api_lookup; SELECT COUNT(*) FROM public.api_endpoint_lookup; SELECT COUNT(*) FROM public.consumer_type_lookup; SELECT COUNT(*) FROM public.tokens";
            using var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = checkSql;
            using var reader = checkCmd.ExecuteReader();
            while (reader.Read())
            {
                var count = reader.GetInt32(0);
                if (count != 0)
                {
                    throw new InvalidOperationException("Table is not empty after truncation.");
                }
            }
        }
    }
}
