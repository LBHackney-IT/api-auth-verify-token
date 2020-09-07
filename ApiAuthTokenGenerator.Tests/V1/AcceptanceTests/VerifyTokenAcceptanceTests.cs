using Amazon.Lambda.APIGatewayEvents;
using ApiAuthTokenGenerator.Tests.V1.TestHelper;
using ApiAuthTokenGenerator.V1.Domain;
using ApiAuthTokenGenerator.V1.Gateways;
using ApiAuthTokenGenerator.V1.Helpers;
using ApiAuthTokenGenerator.V1.UseCase;
using ApiAuthTokenGenerator.V1.UseCase.Interfaces;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace ApiAuthTokenGenerator.Tests.V1.AcceptanceTests
{
    public class VerifyTokenAcceptanceTests
    {
        private LambdaHandler _classUnderTest;
        private Mock<IAuthTokenDatabaseGateway> _mockDatabaseGateway;
        private Mock<IAwsApiGateway> _mockAwsApiGateway;
        private Mock<IServiceProvider> _serviceProvider;
        private string _jwt;
        private readonly Fixture _fixture = new Fixture();
        [SetUp]
        public void Setup()
        {
            _serviceProvider = new Mock<IServiceProvider>();
            _classUnderTest = new LambdaHandler(_serviceProvider.Object);
            _mockDatabaseGateway = new Mock<IAuthTokenDatabaseGateway>();
            _mockAwsApiGateway = new Mock<IAwsApiGateway>();
            Environment.SetEnvironmentVariable("jwtSecret", _fixture.Create<string>());
            var tokenRequest = ValidateJwtTokenHelper.GenerateJwtRequestObject();
            _jwt = new GenerateJwtHelper().GenerateJwtToken(tokenRequest);
            //Needed for logging
            Environment.SetEnvironmentVariable("LambdaEnvironment", "test");

        }
        [Test]
        public void FunctionShouldReturnAPIGatewayCustomAuthorizerPolicyWithAllowEffectWhenTokenIsValid()
        {

            _serviceProvider
                .Setup(x => x.GetService(typeof(IVerifyAccessUseCase)))
                .Returns(new VerifyAccessUseCase(_mockDatabaseGateway.Object, _mockAwsApiGateway.Object));

            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.Headers["Authorization"] = _jwt;
            var apiName = _fixture.Create<string>();
            var consumerName = _fixture.Create<string>();
            var tokenData = new AuthToken
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
            _mockAwsApiGateway.Setup(x => x.GetApiName(It.IsAny<string>())).Returns(apiName);


            var result = _classUnderTest.VerifyToken(lambdaRequest);

            result.Should().BeOfType<APIGatewayCustomAuthorizerResponse>();
            result.PolicyDocument.Statement.First().Effect.Should().Be("Allow");
            result.PrincipalID.Should().Be(consumerName);
        }

        [Test]
        public void FunctionShouldReturnAPIGatewayCustomAuthorizerPolicyWithDenyEffectWhenTokenIsInvalid()
        {

            _serviceProvider
                .Setup(x => x.GetService(typeof(IVerifyAccessUseCase)))
                .Returns(new VerifyAccessUseCase(_mockDatabaseGateway.Object, _mockAwsApiGateway.Object));

            var lambdaRequest = _fixture.Build<APIGatewayCustomAuthorizerRequest>().Create();
            lambdaRequest.Headers["Authorization"] = _jwt;
            //change jwt secret to simulate failure of validating token
            Environment.SetEnvironmentVariable("jwtSecret", _fixture.Create<string>());
            var result = _classUnderTest.VerifyToken(lambdaRequest);

            result.Should().BeOfType<APIGatewayCustomAuthorizerResponse>();
            result.PolicyDocument.Statement.First().Effect.Should().Be("Deny");
        }
    }
}
