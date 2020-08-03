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
            defaultRecordRetrieved.ApiNameLookupId.Should().Be(tokenRequest.ApiName);
        }
    }
}
