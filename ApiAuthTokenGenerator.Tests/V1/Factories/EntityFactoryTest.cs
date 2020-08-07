using ApiAuthTokenGenerator.V1.Factories;
using ApiAuthTokenGenerator.V1.Domain;
using FluentAssertions;
using NUnit.Framework;
using AutoFixture;
using ApiAuthTokenGenerator.V1.Infrastructure;

namespace ApiAuthTokenGenerator.Tests.V1.Factories
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
            response.ConsumerType.Should().Be(consumerData.TypeName);
            response.Id.Should().Be(tokenData.Id);
            response.ExpirationDate.Should().Be(tokenData.ExpirationDate);
            response.Environment.Should().Be(tokenData.Environment);
            response.ConsumerName.Should().Be(tokenData.ConsumerName);
            response.Valid.Should().Be(tokenData.Valid);
        }
    }
}
