using ApiAuthTokenGenerator.V1.Boundary;
using ApiAuthTokenGenerator.V1.Domain;
using ApiAuthTokenGenerator.V1.Helpers;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.Tests.V1.Helpers
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
        [Ignore("value can vary if used local or from AWS API Gateway")]
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
        [Ignore("Redundant")]
        public void IfApiNametInRequestDoesNotMatchDatabaseRecordShouldReturnFalse()
        {
            var request = GenerateAuthorizerRequest();
            var apiName = _faker.Random.Word();
            var tokenData = GenerateTokenData(request, _faker.Random.Word());
            var result = VerifyAccessHelper.ShouldHaveAccess(request, tokenData, apiName);
            result.Should().BeFalse();
        }
        [Test]
        [Ignore("Value is not path to endpoint")]
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
                ExpirationDate = null,
                Enabled = true,
            };
        }

    }
}
