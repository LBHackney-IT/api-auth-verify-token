using ApiAuthTokenManagement.V1.Boundary.Request;
using ApiAuthTokenManagement.V1.Domain;
using ApiAuthTokenManagement.V1.Helpers;
using Bogus;
using FluentAssertions;
using NUnit.Framework;

namespace ApiAuthTokenManagement.Tests.V1.Helpers
{
    public class VerifyAccessHelperTests
    {
        private readonly Faker _faker = new Faker();
        [Test]
        public void IfTokenIsNotValidShouldHaveAccessIsFalse()
        {
            var request = GenerateAuthorizerRequest();
            var apiName = _faker.Random.Word();
            var tokenData = GenerateTokenData(request, apiName);
            tokenData.Enabled = false;
            var result = VerifyAccessHelper.ShouldHaveAccess(request, tokenData, apiName);
            result.Should().BeFalse();
        }
        [Test]
        public void IfTokenExpirationDateIsNullShouldReturnTrue()
        {
            var request = GenerateAuthorizerRequest();
            var apiName = _faker.Random.Word();
            var tokenData = GenerateTokenData(request, apiName);
            var result = VerifyAccessHelper.ShouldHaveAccess(request, tokenData, apiName);
            result.Should().BeTrue();
        }
        [Test]
        public void IfTokenExpirationDateHasPassedShouldReturnFalse()
        {
            var request = GenerateAuthorizerRequest();
            var apiName = _faker.Random.Word();
            var tokenData = GenerateTokenData(request, apiName);
            tokenData.ExpirationDate = _faker.Date.Past();
            var result = VerifyAccessHelper.ShouldHaveAccess(request, tokenData, apiName);
            result.Should().BeFalse();
        }
        [Test]
        public void IfEnvironmentInRequestDoesNotMatchDatabaseRecordShouldReturnFalse()
        {
            var request = GenerateAuthorizerRequest();
            var apiName = _faker.Random.Word();
            var tokenData = GenerateTokenData(request, apiName);
            tokenData.Environment = _faker.Random.Word();
            var result = VerifyAccessHelper.ShouldHaveAccess(request, tokenData, apiName);
            result.Should().BeFalse();
        }
        [Test]
        public void IfApiNametInRequestDoesNotMatchDatabaseRecordShouldReturnFalse()
        {
            var request = GenerateAuthorizerRequest();
            var apiName = _faker.Random.Word();
            var tokenData = GenerateTokenData(request, _faker.Random.Word());
            var result = VerifyAccessHelper.ShouldHaveAccess(request, tokenData, apiName);
            result.Should().BeFalse();
        }
        [Test]
        public void IfApiEndpointNametInRequestDoesNotMatchDatabaseRecordShouldReturnFalse()
        {
            var request = GenerateAuthorizerRequest();
            var apiName = _faker.Random.Word();
            var tokenData = GenerateTokenData(request, apiName);
            tokenData.ApiEndpointName = _faker.Random.Word();
            var result = VerifyAccessHelper.ShouldHaveAccess(request, tokenData, apiName);
            result.Should().BeFalse();
        }
        [Test]
        public void IfHttpMethodTypetInRequestDoesNotMatchDatabaseRecordShouldReturnFalse()
        {
            var request = GenerateAuthorizerRequest();
            var apiName = _faker.Random.Word();
            var tokenData = GenerateTokenData(request, apiName);
            tokenData.HttpMethodType = _faker.Random.AlphaNumeric(6);
            var result = VerifyAccessHelper.ShouldHaveAccess(request, tokenData, apiName);
            result.Should().BeFalse();
        }
        [Test]
        public void IfAllParametersMatchShouldReturnTrue()
        {
            var request = GenerateAuthorizerRequest();
            var apiName = _faker.Random.Word();
            var tokenData = GenerateTokenData(request, apiName);
            var result = VerifyAccessHelper.ShouldHaveAccess(request, tokenData, apiName);
            result.Should().BeTrue();
        }
        private AuthorizerRequest GenerateAuthorizerRequest()
        {
            return new AuthorizerRequest
            {
                ApiEndpointName = _faker.Random.Word(),
                ApiAwsId = _faker.Random.Word(),
                HttpMethodType = _faker.Random.AlphaNumeric(6),
                Environment = _faker.Random.Word(),
            };
        }

        private static AuthToken GenerateTokenData(AuthorizerRequest request, string apiName)
        {
            return new AuthToken
            {
                ApiEndpointName = request.ApiEndpointName,
                ApiName = apiName,
                Environment = request.Environment,
                HttpMethodType = request.HttpMethodType,
                ExpirationDate = null,
                Enabled = true,
            };
        }

    }
}
