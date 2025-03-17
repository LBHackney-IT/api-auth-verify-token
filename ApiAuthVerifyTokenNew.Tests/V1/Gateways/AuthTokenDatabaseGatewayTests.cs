using AutoFixture;
using ApiAuthVerifyTokenNew.V1.Domain;
using ApiAuthVerifyTokenNew.V1.Gateways;
using FluentAssertions;
using NUnit.Framework;
using System;
using ApiAuthVerifyTokenNew.V1.Infrastructure;

namespace ApiAuthVerifyTokenNew.Tests.V1.Gateways
{
    //For instruction on how to run tests please see the wiki: https://github.com/LBHackney-IT/lbh-base-api/wiki/Running-the-test-suite.
    [TestFixture]
    public class AuthTokenDatabaseGatewayTests : DatabaseTests
    {
        private readonly Fixture _fixture = new Fixture();
        private AuthTokenDatabaseGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new AuthTokenDatabaseGateway(DatabaseContext);
        }

        [Test]
        public void TokenRecordShouldBeRetrievedIfValidIdIsSupplied()
        {
            var tokenDataInDb = AddTokenRecordToTheDatabase();

            var result = _classUnderTest.GetTokenData(tokenDataInDb.Id);

            result.Should().BeOfType<AuthTokenServiceFlow>();
            result.Should().NotBeNull();
            result.Id.Should().Be(tokenDataInDb.Id);
            result.ExpirationDate.Should().Be(tokenDataInDb.ExpirationDate);
            result.Environment.Should().Be(tokenDataInDb.Environment);
            result.ConsumerName.Should().Be(tokenDataInDb.ConsumerName);
            result.ConsumerType.Should().Be(tokenDataInDb.ConsumerType);
            result.Enabled.Should().Be(tokenDataInDb.Enabled);
            result.ApiEndpointName.Should().Be(tokenDataInDb.ApiEndpointName);
            result.ApiName.Should().Be(tokenDataInDb.ApiName);
            result.HttpMethodType.Should().Be(tokenDataInDb.HttpMethodType);
        }
        [Test]
        public void ShouldThrowAnExceptionIfTokenMatchIsNotFound()
        {
            Func<AuthTokenServiceFlow> testDelegate = () => _classUnderTest.GetTokenData(_fixture.Create<int>());
            testDelegate.Should().Throw<TokenDataNotFoundException>();
        }
        private AuthTokenServiceFlow AddTokenRecordToTheDatabase()
        {
            var api = _fixture.Build<ApiNameLookup>().Create();
            DatabaseContext.Add(api);

            var apiEndpoint = _fixture.Build<ApiEndpointNameLookup>()
                .With(x => x.ApiLookupId, api.Id).Create();
            DatabaseContext.Add(apiEndpoint);

            var consumerType = _fixture.Build<ConsumerTypeLookup>().Create();
            DatabaseContext.Add(consumerType);

            var tokenData = _fixture.Build<AuthTokens>()
                .With(x => x.ApiEndpointNameLookupId, apiEndpoint.Id)
                .With(x => x.ApiLookupId, api.Id)
                .With(x => x.ConsumerTypeLookupId, consumerType.Id)
                .Create();
            DatabaseContext.Add(tokenData);

            DatabaseContext.SaveChanges();
            return new AuthTokenServiceFlow
            {
                ApiEndpointName = apiEndpoint.ApiEndpointName,
                HttpMethodType = tokenData.HttpMethodType,
                ApiName = api.ApiName,
                ConsumerType = consumerType.TypeName,
                ConsumerName = tokenData.ConsumerName,
                Environment = tokenData.Environment,
                ExpirationDate = tokenData.ExpirationDate,
                Enabled = tokenData.Enabled,
                Id = tokenData.Id
            };
        }
    }
}
