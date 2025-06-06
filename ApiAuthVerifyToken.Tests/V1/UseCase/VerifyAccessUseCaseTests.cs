using ApiAuthVerifyToken.Tests.V1.TestHelper;
using ApiAuthVerifyToken.V1.Boundary;
using ApiAuthVerifyToken.V1.Domain;
using ApiAuthVerifyToken.V1.Gateways;
using ApiAuthVerifyToken.V1.UseCase;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ApiAuthVerifyToken.Tests.V1.UseCase
{
    public class VerifyAccessUseCaseTests
    {
        private VerifyAccessUseCase _classUnderTest;
        private Mock<IAuthTokenDatabaseGateway> _mockDatabaseGateway;
        private Mock<IDynamoDbGateway> _mockDynamoDbGateway;
        private readonly Faker _faker = new Faker();
        private readonly Fixture _fixture = new Fixture();
        [SetUp]
        public void Setup()
        {
            _mockDatabaseGateway = new Mock<IAuthTokenDatabaseGateway>();
            _mockDynamoDbGateway = new Mock<IDynamoDbGateway>();
            _classUnderTest = new VerifyAccessUseCase(_mockDatabaseGateway.Object, _mockDynamoDbGateway.Object);
            Environment.SetEnvironmentVariable("jwtSecret", _faker.Random.AlphaNumeric(50));
            Environment.SetEnvironmentVariable("hackneyUserAuthTokenJwtSecret", _faker.Random.AlphaNumeric(50));
        }
        #region Service Auth Flow

        [Test]
        public void VerifyThatUseCaseCallsGateway()
        {
            var request = GenerateAuthorizerRequest(GenerateJwtHelper.GenerateJwtToken());

            _mockDatabaseGateway.Setup(x => x.GetTokenData(It.IsAny<int>())).Returns(new AuthTokenServiceFlow());
            _classUnderTest.ExecuteServiceAuth(request);

            _mockDatabaseGateway.Verify(x => x.GetTokenData(It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void ShouldReturnFalseIfTokenIsNotValid()
        {
            var request = GenerateAuthorizerRequest(GenerateJwtHelper.GenerateJwtToken());
            //change key to simulate failed validation
            Environment.SetEnvironmentVariable("jwtSecret", _faker.Random.AlphaNumeric(50));
            var result = _classUnderTest.ExecuteServiceAuth(request);
            result.Allow.Should().BeFalse();
            result.User.Should().Be("user");
        }
        [Test]
        public void ShouldAllowAccessIfTokenIsValidAndDataMatchesRecords()
        {
            var request = GenerateAuthorizerRequest(GenerateJwtHelper.GenerateJwtToken());
            var apiName = _faker.Random.Word();
            var consumerName = _faker.Random.Word();
            var tokenData = new AuthTokenServiceFlow
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

            var result = _classUnderTest.ExecuteServiceAuth(request);

            result.Allow.Should().BeTrue();
            result.User.Should().Be(consumerName + tokenData.Id);
        }
        [Test]
        public void ShouldNotAllowAccessIfTokenIsValidButDoesNotMatchTokenDataRecords()
        {
            var request = GenerateAuthorizerRequest(GenerateJwtHelper.GenerateJwtToken());

            _mockDatabaseGateway.Setup(x => x.GetTokenData(It.IsAny<int>())).Returns(new AuthTokenServiceFlow());
            var apiName = _faker.Random.Word();

            var result = _classUnderTest.ExecuteServiceAuth(request);

            result.Allow.Should().BeFalse();
            result.User.Should().Be("0");
        }
        #endregion

        #region User Auth Flow
        [Test]
        public void VerifyThatUseCaseForUserAuthCallsGateway()
        {
            var groups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            var request = GenerateAuthorizerRequest(GenerateJwtHelper.GenerateJwtTokenUserFlow(groups));
            var apiName = _faker.Random.Word();
            var dbData = _fixture.Build<APIDataUserFlow>().With(x => x.ApiName, apiName).Create();
            _mockDynamoDbGateway.Setup(x => x.GetAPIDataByApiGatewayIdAsync(request.ApiAwsId)).Returns(dbData);

            _classUnderTest.ExecuteUserAuth(request);

            _mockDynamoDbGateway.Verify(x => x.GetAPIDataByApiGatewayIdAsync(request.ApiAwsId), Times.Once);
        }

        [Test]
        public void ShouldAllowAccessIfGroupsUserBelongsToMatchDbGroups()
        {
            var groups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            var request = GenerateAuthorizerRequest(GenerateJwtHelper.GenerateJwtTokenUserFlow(groups));
            var apiName = _faker.Random.Word();

            var dbData = _fixture.Build<APIDataUserFlow>()
                .With(x => x.AllowedGroups, groups)
                .With(x => x.Environment, request.Environment)
                .With(x => x.AwsAccount, request.AwsAccountId)
                .With(x => x.ApiName, apiName).Create();

            _mockDynamoDbGateway.Setup(x => x.GetAPIDataByApiGatewayIdAsync(request.ApiAwsId)).Returns(dbData);

            var result = _classUnderTest.ExecuteUserAuth(request);

            result.Allow.Should().BeTrue();
        }
        [Test]
        public void ShouldDenyAccessIfRequestDataDoesNotMatchDbData()
        {
            var groups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            var request = GenerateAuthorizerRequest(GenerateJwtHelper.GenerateJwtTokenUserFlow(groups));
            var apiName = _faker.Random.Word();
            //no matching environment or aws account should result in deny
            var dbData = _fixture.Build<APIDataUserFlow>()
                .With(x => x.AllowedGroups, groups)
                .With(x => x.ApiName, apiName).Create();

            _mockDynamoDbGateway.Setup(x => x.GetAPIDataByApiGatewayIdAsync(request.ApiAwsId)).Returns(dbData);

            var result = _classUnderTest.ExecuteUserAuth(request);

            result.Allow.Should().BeFalse();
        }
        #endregion
        private AuthorizerRequest GenerateAuthorizerRequest(string jwt)
        {
            return new AuthorizerRequest
            {
                ApiEndpointName = _faker.Random.Word(),
                ApiAwsId = _faker.Random.Word(),
                Environment = _faker.Random.Word(),
                Token = jwt,
                AwsAccountId = _faker.Random.Word()
            };
        }
    }
}
