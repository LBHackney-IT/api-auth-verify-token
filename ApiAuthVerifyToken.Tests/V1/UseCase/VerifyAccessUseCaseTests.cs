using ApiAuthVerifyToken.Tests.V1.TestHelper;
using ApiAuthVerifyToken.V1.Boundary;
using ApiAuthVerifyToken.V1.Domain;
using ApiAuthVerifyToken.V1.Gateways;
using ApiAuthVerifyToken.V1.UseCase;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;

namespace ApiAuthVerifyToken.Tests.V1.UseCase
{
    public class VerifyAccessUseCaseTests
    {
        private VerifyAccessUseCase _classUnderTest;
        private Mock<IAuthTokenDatabaseGateway> _mockDatabaseGateway;
        private Mock<IAwsApiGateway> _mockAwsApiGateway;
        private Faker _faker;
        private string _jwt;
        [SetUp]
        public void Setup()
        {
            _mockDatabaseGateway = new Mock<IAuthTokenDatabaseGateway>();
            _mockAwsApiGateway = new Mock<IAwsApiGateway>();
            _classUnderTest = new VerifyAccessUseCase(_mockDatabaseGateway.Object, _mockAwsApiGateway.Object);
            _faker = new Faker();
            Environment.SetEnvironmentVariable("jwtSecret", _faker.Random.AlphaNumeric(25));
            _jwt = GenerateJwtHelper.GenerateJwtToken();
        }

        [Test]
        public void VerifyThatUseCaseCallsGateway()
        {
            var request = GenerateAuthorizerRequest();

            _mockDatabaseGateway.Setup(x => x.GetTokenData(It.IsAny<int>())).Returns(new AuthToken());
            _classUnderTest.Execute(request);

            _mockDatabaseGateway.Verify(x => x.GetTokenData(It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void ShouldReturnFalseIfTokenIsNotValid()
        {
            var request = GenerateAuthorizerRequest();
            //change key to simulate failed validation
            Environment.SetEnvironmentVariable("jwtSecret", _faker.Random.AlphaNumeric(16));
            var result = _classUnderTest.Execute(request);
            result.Allow.Should().BeFalse();
            result.User.Should().BeNull();
        }
        [Test]
        public void ShouldAllowAccessIfTokenIsValidAndDataMatchesRecords()
        {
            var request = GenerateAuthorizerRequest();
            var apiName = _faker.Random.Word();
            var consumerName = _faker.Random.Word();
            _mockAwsApiGateway.Setup(x => x.GetApiName(It.IsAny<string>())).Returns(apiName);
            var tokenData = new AuthToken
            {
                ApiEndpointName = request.ApiEndpointName,
                ApiName = apiName,
                HttpMethodType = request.HttpMethodType,
                Environment = request.Environment,
                ConsumerName = consumerName,
                Enabled = true,
                ExpirationDate = null
            };

            _mockDatabaseGateway.Setup(x => x.GetTokenData(It.IsAny<int>())).Returns(tokenData);

            var result = _classUnderTest.Execute(request);

            result.Allow.Should().BeTrue();
            result.User.Should().Be(consumerName);
        }
        [Test]
        public void ShouldNotAllowAccessIfTokenIsValidButDoesNotMatchTokenDataRecords()
        {
            var request = GenerateAuthorizerRequest();

            _mockDatabaseGateway.Setup(x => x.GetTokenData(It.IsAny<int>())).Returns(new AuthToken());
            var apiName = _faker.Random.Word();
            _mockAwsApiGateway.Setup(x => x.GetApiName(It.IsAny<string>())).Returns(apiName);

            var result = _classUnderTest.Execute(request);

            result.Allow.Should().BeFalse();
            result.User.Should().BeNull();
        }
        private AuthorizerRequest GenerateAuthorizerRequest()
        {
            return new AuthorizerRequest
            {
                ApiEndpointName = _faker.Random.Word(),
                ApiAwsId = _faker.Random.Word(),
                Environment = _faker.Random.Word(),
                Token = _jwt
            };
        }
    }
}
