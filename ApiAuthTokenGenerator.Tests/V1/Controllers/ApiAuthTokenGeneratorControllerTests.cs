using ApiAuthTokenGenerator.V1.Boundary.Request;
using ApiAuthTokenGenerator.V1.Boundary.Response;
using ApiAuthTokenGenerator.V1.Controllers;
using ApiAuthTokenGenerator.V1.UseCase.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.Tests.V1.Controllers
{
    public class ApiAuthTokenGeneratorControllerTests
    {
        private ApiAuthTokenGeneratorController _classUnderTest;
        private Mock<IPostTokenUseCase> _mockPostTokenUseCase;
        private Mock<IGetAllUseCase> _mockGetAllUseCase;
        private Mock<IGetByIdUseCase> _mockGetByIdUseCase;

        [SetUp]
        public void Setup()
        {
            _mockGetAllUseCase = new Mock<IGetAllUseCase>();
            _mockGetByIdUseCase = new Mock<IGetByIdUseCase>();
            _mockPostTokenUseCase = new Mock<IPostTokenUseCase>();
            _classUnderTest = new ApiAuthTokenGeneratorController(_mockGetAllUseCase.Object, _mockGetByIdUseCase.Object, _mockPostTokenUseCase.Object);
        }

        [Test]
        public void EnsureControllerPostMethodCallsPostTokenUseCase()
        {
            var response = new GenerateTokenResponse();
            _mockPostTokenUseCase.Setup(x => x.Execute(It.IsAny<TokenRequestObject>())).Returns(response);
            _classUnderTest.GenerateToken(It.IsAny<TokenRequestObject>());

            _mockPostTokenUseCase.Verify(x => x.Execute(It.IsAny<TokenRequestObject>()), Times.Once);
        }

        [Test]
        public void ControllerPostMethodShouldReturnResponseOfTypeGenerateTokenResponse()
        {
            var response = new GenerateTokenResponse();
            _mockPostTokenUseCase.Setup(x => x.Execute(It.IsAny<TokenRequestObject>())).Returns(response);
            var result = _classUnderTest.GenerateToken(It.IsAny<TokenRequestObject>()) as CreatedAtActionResult;

            result.Should().NotBeNull();
            result.Value.Should().BeOfType<GenerateTokenResponse>();
        }

        [Test]
        public void ControllerPostMethodShouldReturn201StatusCode()
        {
            var response = new GenerateTokenResponse();
            _mockPostTokenUseCase.Setup(x => x.Execute(It.IsAny<TokenRequestObject>())).Returns(response);
            var result = _classUnderTest.GenerateToken(It.IsAny<TokenRequestObject>()) as CreatedAtActionResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
        }
    }
}
