using System.Linq;
using AutoFixture;
using ApiAuthTokenGenerator.V1.Boundary.Response;
using ApiAuthTokenGenerator.V1.Domain;
using ApiAuthTokenGenerator.V1.Factories;
using ApiAuthTokenGenerator.V1.Gateways;
using ApiAuthTokenGenerator.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ApiAuthTokenGenerator.Tests.V1.UseCase
{
    public class GetAllUseCaseTests
    {
        private Mock<IAuthTokenDatabaseGateway> _mockGateway;
        private GetAllUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _mockGateway = new Mock<IAuthTokenDatabaseGateway>();
            _classUnderTest = new GetAllUseCase(_mockGateway.Object);
            _fixture = new Fixture();
        }
    }
}
