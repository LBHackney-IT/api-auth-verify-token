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
            var dynamoDbLocalServiceUrl = Environment.GetEnvironmentVariable("DynamoDb_LocalServiceUrl") ?? "http://localhost:8000";
            var clientConfig = new AmazonDynamoDBConfig { ServiceURL = dynamoDbLocalServiceUrl };
            DynamoDBClient = new AmazonDynamoDBClient(clientConfig);
            try
            {
                var request = new CreateTableRequest("APIAuthenticatorData",
                new List<KeySchemaElement> { new KeySchemaElement("apiName", KeyType.HASH), new KeySchemaElement("environment", KeyType.RANGE) },
                new List<AttributeDefinition> { new AttributeDefinition("apiName", ScalarAttributeType.S), new AttributeDefinition("environment", ScalarAttributeType.S) },
                new ProvisionedThroughput(3, 3));

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
