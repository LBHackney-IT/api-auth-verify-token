using AutoFixture;
using ApiAuthTokenGenerator.Tests.V1.Helper;
using ApiAuthTokenGenerator.V1.Domain;
using ApiAuthTokenGenerator.V1.Gateways;
using FluentAssertions;
using NUnit.Framework;
using ApiAuthTokenGenerator.V1.Boundary.Request;
using FluentAssertions.Extensions;
using System.Linq;
using System;
using ApiAuthTokenGenerator.V1.Infrastructure;

namespace ApiAuthTokenGenerator.Tests.V1.Gateways
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
        public void InsertingATokenRecordShouldReturnAnId()
        {
            var tokenRequest = _fixture.Build<TokenRequestObject>().Create();

            var response = _classUnderTest.GenerateToken(tokenRequest);

            response.Should().NotBe(0);
        }
        [Test]
        public void InsertedRecordShouldBeInsertedOnceInTheDatabase()
        {
            var tokenRequest = _fixture.Build<TokenRequestObject>().Create();

            var response = _classUnderTest.GenerateToken(tokenRequest);

            var databaseRecord = DatabaseContext.Tokens.Where(x => x.Id == response);
            var defaultRecordRetrieved = databaseRecord.FirstOrDefault();

            databaseRecord.Count().Should().Be(1);
        }
        [Test]
        public void InsertedRecordShouldBeInTheDatabase()
        {
            var tokenRequest = _fixture.Build<TokenRequestObject>().Create();

            var response = _classUnderTest.GenerateToken(tokenRequest);

            var databaseRecord = DatabaseContext.Tokens.Where(x => x.Id == response);
            var defaultRecordRetrieved = databaseRecord.FirstOrDefault();

            defaultRecordRetrieved.RequestedBy.Should().Be(tokenRequest.RequestedBy);
            defaultRecordRetrieved.Valid.Should().BeTrue();
            defaultRecordRetrieved.ExpirationDate.Should().Be(tokenRequest.ExpiresAt);
            defaultRecordRetrieved.DateCreated.Date.Should().Be(DateTime.Now.Date);
            defaultRecordRetrieved.Environment.Should().Be(tokenRequest.Environment);
            defaultRecordRetrieved.ConsumerTypeLookupId.Should().Be(tokenRequest.ConsumerType);
            defaultRecordRetrieved.ConsumerName.Should().Be(tokenRequest.Consumer);
            defaultRecordRetrieved.AuthorizedBy.Should().Be(tokenRequest.AuthorizedBy);
            defaultRecordRetrieved.ApiEndpointNameLookupId.Should().Be(tokenRequest.ApiEndpoint);
            defaultRecordRetrieved.ApiLookupId.Should().Be(tokenRequest.ApiName);
        }

        [Test]
        public void TokenRecordShouldBeRetrievedIfValidIdIsSupplied()
        {
            var tokenDataInDb = AddTokebRecordToTheDatabase();

            var result = _classUnderTest.GetTokenData(tokenDataInDb.Id);

            result.Should().BeOfType<AuthToken>();
            result.Should().NotBeNull();
            result.Id.Should().Be(tokenDataInDb.Id);
            result.ExpirationDate.Should().Be(tokenDataInDb.ExpirationDate);
            result.Environment.Should().Be(tokenDataInDb.Environment);
            result.ConsumerName.Should().Be(tokenDataInDb.ConsumerName);
            result.ConsumerType.Should().Be(tokenDataInDb.ConsumerType);
            result.Valid.Should().Be(tokenDataInDb.Valid);
            result.ApiEndpointName.Should().Be(tokenDataInDb.ApiEndpointName);
            result.ApiName.Should().Be(tokenDataInDb.ApiName);
        }
        [Test]
        public void ShouldThrowAnExceptionIfTokenMatchIsNotFound()
        {
            Func<AuthToken> testDelegate = () => _classUnderTest.GetTokenData(_fixture.Create<int>());
            testDelegate.Should().Throw<TokenDataNotFoundException>();
        }
        private AuthToken AddTokebRecordToTheDatabase()
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
            return new AuthToken
            {
                ApiEndpointName = apiEndpoint.ApiEndpointName,
                ApiName = api.ApiName,
                ConsumerType = consumerType.TypeName,
                ConsumerName = tokenData.ConsumerName,
                Environment = tokenData.Environment,
                ExpirationDate = tokenData.ExpirationDate,
                Valid = tokenData.Valid,
                Id = tokenData.Id
            };
        }
    }
}
