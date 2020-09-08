using System;
using System.Globalization;
using ApiAuthTokenManagement.Tests.V1.TestHelper;
using ApiAuthTokenManagement.V1.UseCase;
using Bogus;
using FluentAssertions;
using NUnit.Framework;

namespace ApiAuthTokenManagement.Tests.V1.UseCase
{
    public class GenerateJwtHelperTests
    {
        private Faker _faker;
        private GenerateJwt _classUnderTest;
        private string _jwtSecret;
        [SetUp]
        public void Setup()
        {
            _faker = new Faker();
            _jwtSecret = _faker.Random.AlphaNumeric(20);
            Environment.SetEnvironmentVariable("jwtSecret", _jwtSecret);
            _classUnderTest = new GenerateJwt();
        }
        [Test]
        public void CanGenerateJwtToken()
        {
            var jwtRequest = ValidateJwtTokenHelper.GenerateJwtRequestObject();
            var result = _classUnderTest.Execute(jwtRequest);

            result.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void CanGenerateValidJwtTokenWithClaims()
        {
            var jwtRequest = ValidateJwtTokenHelper.GenerateJwtRequestObject();
            var token = _classUnderTest.Execute(jwtRequest);

            var claimsDecrypted = ValidateJwtTokenHelper.GetJwtClaims(token, _jwtSecret);

            claimsDecrypted.Find(x => x.Type == "id").Value.Should().Be(jwtRequest.Id.ToString(CultureInfo.InvariantCulture));
            claimsDecrypted.Find(x => x.Type == "consumerName").Value.Should().Be(jwtRequest.ConsumerName);
            claimsDecrypted.Find(x => x.Type == "consumerType").Value.Should()
                .Be(jwtRequest.ConsumerType.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void CanGenerateJwtTokenWithoutExpiryDate()
        {
            var jwtRequest = ValidateJwtTokenHelper.GenerateJwtRequestObject();
            var result = _classUnderTest.Execute(jwtRequest);
            var token = ValidateJwtTokenHelper.GetToken(result);

            result.Should().NotBeNullOrWhiteSpace();
            token.ValidTo.Date.Should().BeSameDateAs(DateTime.Now.AddYears(10).Date);
        }

        [Test]
        public void CanGenerateJwtTokenWithExpiryDate()
        {
            var jwtRequest = ValidateJwtTokenHelper.GenerateJwtRequestObject(_faker.Date.Future());
            var result = _classUnderTest.Execute(jwtRequest);
            var token = ValidateJwtTokenHelper.GetToken(result);

            result.Should().NotBeNullOrWhiteSpace();
            token.ValidTo.Date.Should().Be(jwtRequest.ExpiresAt.Value.Date);
        }
    }
}
