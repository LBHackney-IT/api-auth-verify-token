using ApiAuthVerifyToken.V1.Boundary;
using ApiAuthVerifyToken.V1.Domain;
using ApiAuthVerifyToken.V1.Helpers;
using Bogus;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthVerifyToken.Tests.V1.Helpers
{
    public class VerifyAccessHelperTests
    {
        private readonly Faker _faker = new Faker();
        private AuthorizerRequest _request;
        private string _apiName;

        [SetUp]
        public void Setup()
        {
            _request = GenerateAuthorizerRequest();
            _apiName = _faker.Random.Word();
        }
        #region ServiceFlow
        [Test]
        public void IfTokenIsNotValidShouldHaveAccessIsFalse()
        {
            var tokenData = GenerateTokenData(_request, _apiName);
            tokenData.Enabled = false;
            var result = VerifyAccessHelper.ShouldHaveAccessServiceFlow(_request, tokenData, _apiName);
            result.Should().BeFalse();
        }
        [Test]
        public void IfTokenExpirationDateIsNullShouldReturnTrue()
        {
            var tokenData = GenerateTokenData(_request, _apiName);
            var result = VerifyAccessHelper.ShouldHaveAccessServiceFlow(_request, tokenData, _apiName);
            result.Should().BeTrue();
        }
        [Test]
        public void IfTokenExpirationDateHasPassedShouldReturnFalse()
        {
            var tokenData = GenerateTokenData(_request, _apiName);
            tokenData.ExpirationDate = _faker.Date.Past();
            var result = VerifyAccessHelper.ShouldHaveAccessServiceFlow(_request, tokenData, _apiName);
            result.Should().BeFalse();
        }
        [Test]
        public void IfEnvironmentInRequestDoesNotMatchDatabaseRecordShouldReturnFalse()
        {
            var tokenData = GenerateTokenData(_request, _apiName);
            tokenData.Environment = _faker.Random.Word();
            var result = VerifyAccessHelper.ShouldHaveAccessServiceFlow(_request, tokenData, _apiName);
            result.Should().BeFalse();
        }
        [Test]
        public void IfApiNametInRequestDoesNotMatchDatabaseRecordShouldReturnFalse()
        {
            var apiName = _faker.Random.Word();
            var tokenData = GenerateTokenData(_request, _faker.Random.Word());
            var result = VerifyAccessHelper.ShouldHaveAccessServiceFlow(_request, tokenData, apiName);
            result.Should().BeFalse();
        }
        [Test]
        public void IfApiEndpointNametInRequestDoesNotMatchDatabaseRecordShouldReturnFalse()
        {
            var tokenData = GenerateTokenData(_request, _apiName);
            tokenData.ApiEndpointName = _faker.Random.Word();
            var result = VerifyAccessHelper.ShouldHaveAccessServiceFlow(_request, tokenData, _apiName);
            result.Should().BeFalse();
        }
        [Test]
        public void IfHttpMethodTypetInRequestDoesNotMatchDatabaseRecordShouldReturnFalse()
        {
            var tokenData = GenerateTokenData(_request, _apiName);
            tokenData.HttpMethodType = _faker.Random.AlphaNumeric(6);
            var result = VerifyAccessHelper.ShouldHaveAccessServiceFlow(_request, tokenData, _apiName);
            result.Should().BeFalse();
        }
        [Test]
        public void IfAllParametersMatchShouldReturnTrue()
        {
            var tokenData = GenerateTokenData(_request, _apiName);
            var result = VerifyAccessHelper.ShouldHaveAccessServiceFlow(_request, tokenData, _apiName);
            result.Should().BeTrue();
        }
        #endregion
        #region UserFlow
        [Test]
        public void IfGroupsInDbDoNotMatchUserGroupsShouldReturnFalse()
        {
            var allowedGroups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            var userGroups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            var dbData = GenerateTokenDataUserFlow(_request, _apiName, allowedGroups);
            var hackneyUser = new HackneyUser() { Groups = JsonConvert.SerializeObject(userGroups) };

            var result = VerifyAccessHelper.ShouldHaveAccessUserFlow(hackneyUser, _request, dbData, _apiName);

            result.Should().BeFalse();
        }
        [Test]
        public void IfGroupsInDbDoMatchUserGroupsShouldReturnTrue()
        {
            var allowedGroups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            var userGroups = allowedGroups;
            var dbData = GenerateTokenDataUserFlow(_request, _apiName, allowedGroups);
            var hackneyUser = new HackneyUser() { Groups = JsonConvert.SerializeObject(userGroups) };
            var result = VerifyAccessHelper.ShouldHaveAccessUserFlow(hackneyUser, _request, dbData, _apiName);

            result.Should().BeTrue();
        }

        [Test]
        public void IfEnvironmentInRequestDoesNotMatchEnvironmentInDbShouldReturnFalse()
        {
            var allowedGroups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            var userGroups = allowedGroups;
            var dbData = GenerateTokenDataUserFlow(_request, _apiName, allowedGroups);
            dbData.Environemnt = _faker.Random.Word();
            var hackneyUser = new HackneyUser() { Groups = JsonConvert.SerializeObject(userGroups) };
            var result = VerifyAccessHelper.ShouldHaveAccessUserFlow(hackneyUser, _request, dbData, _apiName);

            result.Should().BeFalse();
        }
        [Test]
        public void IfAWSAccounttInRequestDoesNotMatchAWSAccountInDbShouldReturnFalse()
        {
            var allowedGroups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            var userGroups = allowedGroups;
            var dbData = GenerateTokenDataUserFlow(_request, _apiName, allowedGroups);
            dbData.AwsAccount = _faker.Random.Word();
            var hackneyUser = new HackneyUser() { Groups = JsonConvert.SerializeObject(userGroups) };
            var result = VerifyAccessHelper.ShouldHaveAccessUserFlow(hackneyUser, _request, dbData, _apiName);

            result.Should().BeFalse();
        }
        #endregion

        private AuthorizerRequest GenerateAuthorizerRequest()
        {
            return new AuthorizerRequest
            {
                ApiEndpointName = _faker.Random.Word(),
                ApiAwsId = _faker.Random.Word(),
                HttpMethodType = _faker.Random.AlphaNumeric(6),
                Environment = _faker.Random.Word(),
                AwsAccountId = _faker.Random.Word()
            };
        }

        private static AuthTokenServiceFlow GenerateTokenData(AuthorizerRequest request, string apiName)
        {
            return new AuthTokenServiceFlow
            {
                ApiEndpointName = request.ApiEndpointName,
                ApiName = apiName,
                Environment = request.Environment,
                HttpMethodType = request.HttpMethodType,
                ExpirationDate = null,
                Enabled = true,
            };
        }
        private static APIDataUserFlow GenerateTokenDataUserFlow(AuthorizerRequest request, string apiName, List<string> groups)
        {
            return new APIDataUserFlow
            {
                ApiName = apiName,
                AwsAccount = request.AwsAccountId,
                Environemnt = request.Environment,
                AllowedGroups = groups
            };
        }

    }
}
