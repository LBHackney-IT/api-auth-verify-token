using System;
using ApiAuthTokenManagement.V1.Boundary.Request;
using ApiAuthTokenManagement.V1.Boundary.Response;
using ApiAuthTokenManagement.V1.Domain;
using ApiAuthTokenManagement.V1.Domain.Exceptions;
using ApiAuthTokenManagement.V1.Gateways;
using ApiAuthTokenManagement.V1.UseCase;
using ApiAuthTokenManagement.V1.UseCase.Interfaces;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ApiAuthTokenManagement.Tests.V1.UseCase
{
    public class PostTokenUseCaseTests
    {
        private PostTokenUseCase _classUnderTest;
        private Mock<IGenerateJwt> _mockGenerateJwtHelper;
        private Mock<IAuthTokenDatabaseGateway> _mockGateway;
        private Faker _faker;
        [SetUp]
        public void Setup()
        {
            _mockGenerateJwtHelper = new Mock<IGenerateJwt>();
            _mockGateway = new Mock<IAuthTokenDatabaseGateway>();
            _classUnderTest = new PostTokenUseCase(_mockGenerateJwtHelper.Object, _mockGateway.Object);
            _faker = new Faker();
        }
        [Test]
        public void UseCaseShouldCallHelperMethodToGenerateJwtToken()
        {
            var tokenRequest = GetTokenRequestObject();
            var jwtTokenResult = _faker.Random.AlphaNumeric(20);
            _mockGateway.Setup(x => x.GenerateToken(tokenRequest)).Returns(_faker.Random.Int(1, 100));
            _mockGenerateJwtHelper.Setup(x => x.Execute(It.IsAny<JwtTokenRequest>())).Returns(jwtTokenResult);

            _classUnderTest.Execute(tokenRequest);

            _mockGenerateJwtHelper.Verify(x => x.Execute(It.IsAny<JwtTokenRequest>()), Times.Once);
        }
        [Test]
        public void UseCaseShouldCallGatewayToInsertTokenData()
        {
            var tokenRequest = GetTokenRequestObject();
            _mockGateway.Setup(x => x.GenerateToken(tokenRequest)).Returns(_faker.Random.Int(1, 100));
            var jwtTokenResult = _faker.Random.AlphaNumeric(20);
            _mockGenerateJwtHelper.Setup(x => x.Execute(It.IsAny<JwtTokenRequest>())).Returns(jwtTokenResult);
            _classUnderTest.Execute(tokenRequest);

            _mockGateway.Verify(x => x.GenerateToken(It.IsAny<TokenRequestObject>()), Times.Once);
        }

        [Test]
        public void VerifyThatHelperIsNotCalledIfGatewayFailsToInsertRecordAndExceptionIsThrown()
        {
            var tokenRequest = GetTokenRequestObject();
            _mockGateway.Setup(x => x.GenerateToken(tokenRequest)).Returns(0);

            Func<GenerateTokenResponse> testDelegate = () => _classUnderTest.Execute(tokenRequest);
            testDelegate.Should().Throw<TokenNotInsertedException>();
            _mockGenerateJwtHelper.Verify(x => x.Execute(It.IsAny<JwtTokenRequest>()), Times.Never);
        }

        [Test]
        public void VerifyThatExceptionIsThrownIfTokenIsNotGenerated()
        {
            var tokenRequest = GetTokenRequestObject();
            _mockGateway.Setup(x => x.GenerateToken(tokenRequest)).Returns(_faker.Random.Int(1, 100));
            _mockGenerateJwtHelper.Setup(x => x.Execute(It.IsAny<JwtTokenRequest>())).Returns(() => null);

            Func<GenerateTokenResponse> testDelegate = () => _classUnderTest.Execute(tokenRequest);
            testDelegate.Should().Throw<JwtTokenNotGeneratedException>();
        }

        [Test]
        public void GenerateTokenResponseIsReturnedWhenAllOperationsAreSuccessful()
        {
            var tokenRequest = GetTokenRequestObject();
            var tokenId = _faker.Random.Int(1, 100);
            _mockGateway.Setup(x => x.GenerateToken(tokenRequest)).Returns(tokenId);
            var jwtTokenResult = _faker.Random.AlphaNumeric(20);
            _mockGenerateJwtHelper.Setup(x => x.Execute(It.IsAny<JwtTokenRequest>())).Returns(jwtTokenResult);

            var response = _classUnderTest.Execute(tokenRequest);

            response.Should().NotBeNull();
            response.Token.Should().Be(jwtTokenResult);
            response.Id.Should().Be(tokenId);
            response.GeneratedAt.Date.Should().BeSameDateAs(DateTime.Now.Date);
            response.ExpiresAt.Should().Be(tokenRequest.ExpiresAt);
        }

        private TokenRequestObject GetTokenRequestObject()
        {
            return new TokenRequestObject
            {
                Consumer = _faker.Random.String(),
                ConsumerType = _faker.Random.Int(5),
                ExpiresAt = _faker.Date.Future()
            };
        }
    }
}
