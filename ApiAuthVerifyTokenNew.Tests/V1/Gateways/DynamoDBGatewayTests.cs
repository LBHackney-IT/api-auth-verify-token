using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using ApiAuthVerifyToken.V1.Domain;
using ApiAuthVerifyToken.V1.Gateways;
using ApiAuthVerifyToken.V1.Infrastructure;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ApiAuthVerifyTokenNew.Tests.V1.Gateways
{
    [TestFixture]
    public class DynamoDBGatewayTests : DynamoDBTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Faker _faker = new Faker();
        // private DynamoDBContext _dynamoDBContext;
        private DynamoDBGateway _classUnderTest;
        //    private AmazonDynamoDBClient _client;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new DynamoDBGateway(DynamoDbContext);
        }

        [Test]
        public void VerifyThatGatewayCanRetrieveData()
        {
            var apiData = _fixture.Create<APIDataUserFlowDbEntity>();
            AddDataToDynamoDb(apiData);

            var result = _classUnderTest.GetAPIDataByNameAndEnvironmentAsync(apiData.ApiName, apiData.Environment);

            result.Should().BeEquivalentTo(apiData);
        }

        [Test]
        public void VerifyThatGatewayCanRetrieveDataFilteredBasedOnQuery()
        {
            var apiData = _fixture.Create<APIDataUserFlowDbEntity>();
            AddDataToDynamoDb(apiData);

            var otherApiData = _fixture.Create<APIDataUserFlowDbEntity>();
            AddDataToDynamoDb(otherApiData);

            var result = _classUnderTest.GetAPIDataByNameAndEnvironmentAsync(apiData.ApiName, apiData.Environment);

            result.Should().BeEquivalentTo(apiData);
        }

        [Test]
        public void VerifyThatGatewayThrowsExceptionWhenNoMatchIsFound()
        {

            Func<APIDataUserFlow> testDelegate = () => _classUnderTest.GetAPIDataByNameAndEnvironmentAsync(_faker.Random.Word(), _faker.Random.Word());

            testDelegate.Should().Throw<APIEntryNotFoundException>();
        }

        private void AddDataToDynamoDb(APIDataUserFlowDbEntity apiData)
        {
            Dictionary<string, AttributeValue> attributes = new Dictionary<string, AttributeValue>();
            attributes["apiName"] = new AttributeValue { S = apiData.ApiName };
            attributes["environment"] = new AttributeValue { S = apiData.Environment };
            attributes["awsAccount"] = new AttributeValue { S = apiData.AwsAccount };
            attributes["allowedGroups"] = new AttributeValue { SS = new List<string>(apiData.AllowedGroups) };

            PutItemRequest request = new PutItemRequest
            {
                TableName = "APIAuthenticatorData",
                Item = attributes
            };

            DynamoDBClient.PutItemAsync(request).GetAwaiter().GetResult();
        }
    }
}
