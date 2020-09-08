using System;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using ApiAuthTokenManagement.Tests.V1.TestHelper;
using ApiAuthTokenManagement.V1.Controllers;
using ApiAuthTokenManagement.V1.Domain;
using ApiAuthTokenManagement.V1.Gateways;
using ApiAuthTokenManagement.V1.UseCase;
using ApiAuthTokenManagement.V1.UseCase.Interfaces;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ApiAuthTokenManagement.Tests.V1.IntegrationTests
{
    public class VerifyTokenIntegrationTests
    {
        private TokenVerifierHandler _classUnderTest;
        private Mock<IAuthTokenDatabaseGateway> _mockDatabaseGateway;
        private Mock<IAwsApiGateway> _mockAwsApiGateway;
        private Mock<IServiceProvider> _serviceProvider;
        private string _jwt;
        private readonly Fixture _fixture = new Fixture();
        [SetUp]
        public void Setup()
        {
            _serviceProvider = new Mock<IServiceProvider>();
            _classUnderTest = new TokenVerifierHandler(_serviceProvider.Object);
            _mockDatabaseGateway = new Mock<IAuthTokenDatabaseGateway>();
            _mockAwsApiGateway = new Mock<IAwsApiGateway>();
            Environment.SetEnvironmentVariable("jwtSecret", _fixture.Create<string>());
            var tokenRequest = ValidateJwtTokenHelper.GenerateJwtRequestObject();
            _jwt = new GenerateJwt().Execute(tokenRequest);
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
