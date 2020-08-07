using ApiAuthTokenGenerator.Tests.V1.TestHelper;
using ApiAuthTokenGenerator.V1.Boundary;
using ApiAuthTokenGenerator.V1.Helpers;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.Tests.V1.Helpers
{
    public class GenerateJwtHelperTests
    {
        private Faker _faker;
        private GenerateJwtHelper _classUnderTest;
        private string _jwtSecret;
        [SetUp]
        public void Setup()
        {
            _faker = new Faker();
            _jwtSecret = _faker.Random.AlphaNumeric(20);
            Environment.SetEnvironmentVariable("jwtSecret", _jwtSecret);
            _classUnderTest = new GenerateJwtHelper();
        }
        [Test]
        public void CanGenerateJwtToken()
        {
            var jwtRequest = ValidateJwtTokenHelper.GenerateJwtRequestObject();
            var result = _classUnderTest.GenerateJwtToken(jwtRequest);

            result.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void CanGenerateValidJwtTokenWithClaims()
        {
            var jwtRequest = ValidateJwtTokenHelper.GenerateJwtRequestObject();
            var token = _classUnderTest.GenerateJwtToken(jwtRequest);

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
            var result = _classUnderTest.GenerateJwtToken(jwtRequest);
            var token = ValidateJwtTokenHelper.GetToken(result);

            result.Should().NotBeNullOrWhiteSpace();
            token.ValidTo.Date.Should().BeSameDateAs(DateTime.Now.AddYears(10).Date);
        }

        [Test]
        public void CanGenerateJwtTokenWithExpiryDate()
        {
            var jwtRequest = ValidateJwtTokenHelper.GenerateJwtRequestObject(_faker.Date.Future());
            var result = _classUnderTest.GenerateJwtToken(jwtRequest);
            var token = ValidateJwtTokenHelper.GetToken(result);

            result.Should().NotBeNullOrWhiteSpace();
            token.ValidTo.Date.Should().Be((DateTime) jwtRequest.ExpiresAt.Value.Date);
        }
    }
}
