using Amazon.Lambda.APIGatewayEvents;
using ApiAuthVerifyToken.V1.Domain;
using ApiAuthVerifyToken.V1.Gateways;
using ApiAuthVerifyToken.V1.UseCase;
using ApiAuthVerifyToken.V1.UseCase.Interfaces;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using ApiAuthVerifyToken.Tests.V1.TestHelper;
using ApiAuthVerifyToken.V1.Boundary;
using Bogus;
using System.Collections.Generic;

namespace ApiAuthVerifyToken.Tests.V1.AcceptanceTests
{
    public class VerifyTokenAcceptanceTests
    {
        private VerifyTokenHandler _classUnderTest;
        private Mock<IAuthTokenDatabaseGateway> _mockDatabaseGateway;
        private Mock<IServiceProvider> _serviceProvider;
        private Mock<IDynamoDbGateway> _mockDynamoDbGateway;
        private string _jwtServiceFlow;
        private string _jwtUserFlow;
        private readonly Fixture _fixture = new Fixture();
        private readonly Faker _faker = new Faker();
        private List<string> _allowedGroups;
        [SetUp]
        public void Setup()
        {
            _serviceProvider = new Mock<IServiceProvider>();
            _classUnderTest = new VerifyTokenHandler(_serviceProvider.Object);
            _mockDatabaseGateway = new Mock<IAuthTokenDatabaseGateway>();
            _mockDynamoDbGateway = new Mock<IDynamoDbGateway>();
            //set up env vars
            Environment.SetEnvironmentVariable("jwtSecret", _faker.Random.AlphaNumeric(50));
            Environment.SetEnvironmentVariable("hackneyUserAuthTokenJwtSecret", _faker.Random.AlphaNumeric(50));
            //set up JWT tokens
            _allowedGroups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            _jwtServiceFlow = GenerateJwtHelper.GenerateJwtToken();
            _jwtUserFlow = GenerateJwtHelper.GenerateJwtTokenUserFlow(_allowedGroups);
        }
        [Test]
        public void FunctionShouldReturnAPIGatewayCustomAuthorizerPolicyWithAllowEffectWhenTokenIsValid()
        {

            _serviceProvider
                .Setup(x => x.GetService(typeof(IVerifyAccessUseCase)))
                .Returns(new VerifyAccessUseCase(_mockDatabaseGateway.Object, _mockDynamoDbGateway.Object));

            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.Headers["Authorization"] = _jwtServiceFlow;
            var apiName = _fixture.Create<string>();
            var consumerName = _fixture.Create<string>();
            var tokenData = new AuthTokenServiceFlow
            {
                ApiEndpointName = lambdaRequest.RequestContext.Path,
                ApiName = apiName,
                Environment = lambdaRequest.RequestContext.Stage,
                HttpMethodType = lambdaRequest.RequestContext.HttpMethod,
                ConsumerName = consumerName,
                Enabled = true,
                ExpirationDate = null
            };
            _mockDatabaseGateway.Setup(x => x.GetTokenData(It.IsAny<int>())).Returns(tokenData);

            var result = _classUnderTest.VerifyToken(lambdaRequest);

            result.Should().BeOfType<APIGatewayCustomAuthorizerResponse>();
            result.PolicyDocument.Statement.First().Effect.Should().Be("Allow");
            result.PrincipalID.Should().Be(consumerName + tokenData.Id);
        }

        [Test]
        public void FunctionShouldReturnAPIGatewayCustomAuthorizerPolicyWithDenyEffectWhenTokenIsInvalid()
        {

            _serviceProvider
                .Setup(x => x.GetService(typeof(IVerifyAccessUseCase)))
                .Returns(new VerifyAccessUseCase(_mockDatabaseGateway.Object, _mockDynamoDbGateway.Object));

            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.Headers["Authorization"] = _jwtServiceFlow;
            //change jwt secret to simulate failure of validating token
            Environment.SetEnvironmentVariable("jwtSecret", _faker.Random.AlphaNumeric(50));
            var result = _classUnderTest.VerifyToken(lambdaRequest);

            result.Should().BeOfType<APIGatewayCustomAuthorizerResponse>();
            result.PolicyDocument.Statement.First().Effect.Should().Be("Deny");
        }
        #region UserFlow
        [Test]
        public void FunctionShouldReturnAPIGatewayCustomAuthorizerPolicyWithAllowEffectWhenAuthenticationSuccesdsForUserFlow()
        {

            _serviceProvider
                .Setup(x => x.GetService(typeof(IVerifyAccessUseCase)))
                .Returns(new VerifyAccessUseCase(_mockDatabaseGateway.Object, _mockDynamoDbGateway.Object));

            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.Headers["Authorization"] = _jwtUserFlow;
            var dbData = _fixture.Build<APIDataUserFlow>()
                .With(x => x.AllowedGroups, _allowedGroups)
                .With(x => x.Environment, lambdaRequest.RequestContext.Stage)
                .With(x => x.AwsAccount, lambdaRequest.RequestContext.AccountId)
                .With(x => x.ApiGatewayId, lambdaRequest.RequestContext.ApiId).Create();

            _mockDynamoDbGateway.Setup(x => x.GetAPIDataByApiGatewayIdAsync(lambdaRequest.RequestContext.ApiId)).Returns(dbData);

            var result = _classUnderTest.VerifyToken(lambdaRequest);

            result.Should().BeOfType<APIGatewayCustomAuthorizerResponse>();
            result.PolicyDocument.Statement.First().Effect.Should().Be("Allow");
            result.PrincipalID.Should().NotBeNull();
        }
        [Test]
        public void FunctionShouldReturnAPIGatewayCustomAuthorizerPolicyWithDenyWhenUserGroupsAreNotListedAsAllowed()
        {

            _serviceProvider
                .Setup(x => x.GetService(typeof(IVerifyAccessUseCase)))
                .Returns(new VerifyAccessUseCase(_mockDatabaseGateway.Object, _mockDynamoDbGateway.Object));

            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.Headers["Authorization"] = _jwtUserFlow;
            var apiName = _fixture.Create<string>();
            var nonAllowedGroups = new List<string> { _faker.Random.Word(), _faker.Random.Word() };
            var dbData = _fixture.Build<APIDataUserFlow>()
                .With(x => x.AllowedGroups, nonAllowedGroups)
                .With(x => x.Environment, lambdaRequest.RequestContext.Stage)
                .With(x => x.AwsAccount, lambdaRequest.RequestContext.AccountId)
                .With(x => x.ApiName, apiName).Create();

            _mockDynamoDbGateway.Setup(x => x.GetAPIDataByNameAndEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(dbData);

            var result = _classUnderTest.VerifyToken(lambdaRequest);

            result.Should().BeOfType<APIGatewayCustomAuthorizerResponse>();
            result.PolicyDocument.Statement.First().Effect.Should().Be("Deny");
        }
        #endregion
    }
}
