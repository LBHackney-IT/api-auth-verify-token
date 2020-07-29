using ApiAuthTokenGenerator.V1.Boundary;
using ApiAuthTokenGenerator.V1.Boundary.Request;
using ApiAuthTokenGenerator.V1.Helpers.Interfaces;
using ApiAuthTokenGenerator.V1.UseCase;
using Bogus;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.Tests.V1.UseCase
{
    public class PostTokenUserCaseTests
    {
        private PostTokenUseCase _classUnderTest;
        private Mock<IGenerateJwtHelper> _mockGenerateJwtHelper;
        private Faker _faker;
        [SetUp]
        public void Setup()
        {
            _mockGenerateJwtHelper = new Mock<IGenerateJwtHelper>();
            _classUnderTest = new PostTokenUseCase(_mockGenerateJwtHelper.Object);
            _faker = new Faker();
        }
        [Test]
        public void UseCaseShouldCallHelperMethodToGenerateJwtToken()
        {

            var jwtTokenResult = _faker.Random.AlphaNumeric(20);
            _mockGenerateJwtHelper.Setup(x => x.GenerateJwtToken(It.IsAny<GenerateJwtRequest>())).Returns(jwtTokenResult);

            _classUnderTest.Execute(It.IsAny<TokenRequestObject>());

            _mockGenerateJwtHelper.Verify(x => x.GenerateJwtToken(It.IsAny<GenerateJwtRequest>()), Times.Once);
        }


    }
}
