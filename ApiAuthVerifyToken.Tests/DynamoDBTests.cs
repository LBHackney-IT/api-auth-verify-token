using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthVerifyToken.Tests
{
    [TestFixture]
    public class DynamoDBTests
    {
        protected DynamoDBContext DynamoDbContext { get; private set; }
        protected AmazonDynamoDBClient DynamoDBClient { get; private set; }

        [SetUp]
        public void RunBeforeAnyTests()
        {
            var clientConfig = new AmazonDynamoDBConfig { ServiceURL = Environment.GetEnvironmentVariable("DynamoDb_LocalServiceUrl") };
            DynamoDBClient = new AmazonDynamoDBClient(clientConfig);
            try
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
                            }
                        }
                    },
                    BillingMode = BillingMode.PAY_PER_REQUEST
                };

                DynamoDBClient.CreateTableAsync(request).GetAwaiter().GetResult();
            }
            catch (ResourceInUseException)
            {

            }

            DynamoDbContext = new DynamoDBContext(DynamoDBClient);
        }
        [TearDown]
        public void RunAfterAnyTests()
        {
            DynamoDBClient.Dispose();
        }
    }
}
