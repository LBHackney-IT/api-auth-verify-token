using ApiAuthVerifyToken.V1.Factories;
using ApiAuthVerifyToken.V1.Domain;
using FluentAssertions;
using NUnit.Framework;
using AutoFixture;
using ApiAuthVerifyToken.V1.Infrastructure;

namespace ApiAuthVerifyTokenNew.Tests.V1.Factories
{
    [TestFixture]
    public class EntityFactoryTest
    {
        [Test]
        public void CanMapDatabaseResultsToAuthToken()
        {
            var fixture = new Fixture();
            var tokenData = fixture.Build<AuthTokens>().Create();
            var apiData = fixture.Build<ApiNameLookup>().Create();
            var apiEndpointData = fixture.Build<ApiEndpointNameLookup>().Create();
            var consumerData = fixture.Build<ConsumerTypeLookup>().Create();

            var response = tokenData.ToDomain(apiEndpointData.ApiEndpointName, apiData.ApiName, consumerData.TypeName);

            response.ApiEndpointName.Should().Be(apiEndpointData.ApiEndpointName);
            response.ApiName.Should().Be(apiData.ApiName);
            response.HttpMethodType.Should().Be(tokenData.HttpMethodType);
            response.ConsumerType.Should().Be(consumerData.TypeName);
            response.Id.Should().Be(tokenData.Id);
            response.ExpirationDate.Should().Be(tokenData.ExpirationDate);
            response.Environment.Should().Be(tokenData.Environment);
            response.ConsumerName.Should().Be(tokenData.ConsumerName);
            response.Enabled.Should().Be(tokenData.Enabled);
        }

        [Test]
        public void CanMapDatabaseResultsDynamoDbToAPIData()
        {
            var fixture = new Fixture();
            var apiData = fixture.Build<APIDataUserFlowDbEntity>().Create();
            var response = apiData.ToDomain();

            response.ApiName.Should().Be(apiData.ApiName);
            response.Environment.Should().Be(apiData.Environment);
            response.AwsAccount.Should().Be(apiData.AwsAccount);
            response.AllowedGroups.Should().BeEquivalentTo(apiData.AllowedGroups);
        }
    }
}
