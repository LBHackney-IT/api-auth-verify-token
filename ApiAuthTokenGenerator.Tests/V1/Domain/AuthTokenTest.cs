using System;
using ApiAuthTokenGenerator.V1.Domain;
using FluentAssertions;
using NUnit.Framework;

namespace ApiAuthTokenGenerator.Tests.V1.Domain
{
    [TestFixture]
    public class AuthTokenTest
    {
        private AuthToken _entity;
        [SetUp]
        public void Setup()
        {
            _entity = new AuthToken();
        }
        [Test]
        public void AuthTokenHasAnId()
        {
            _entity.Id.Should().BeGreaterOrEqualTo(0);
        }
        [Test]
        public void AuthTokenHasApiName()
        {
            _entity.ApiName.Should().BeNullOrWhiteSpace();
        }
        [Test]
        public void AuthTokenHasApiEndpointName()
        {
            _entity.ApiEndpointName.Should().BeNullOrWhiteSpace();
        }
        [Test]
        public void AuthTokenHasHttpMethodType()
        {
            _entity.HttpMethodType.Should().BeNullOrWhiteSpace();
        }
        [Test]
        public void AuthTokenHasEnvironment()
        {
            _entity.Environment.Should().BeNullOrWhiteSpace().Should();
        }
        [Test]
        public void AuthTokenHasConsumerName()
        {
            _entity.ConsumerName.Should().BeNullOrWhiteSpace().Should();
        }
        [Test]
        public void AuthTokenHasConsumerType()
        {
            _entity.ConsumerType.Should().BeNullOrWhiteSpace().Should();
        }
        [Test]
        public void AuthTokenHasBooleanValid()
        {
            _entity.Enabled.Should().BeFalse();
        }
        [Test]
        public void AuthTokenHasExpiryDate()
        {
            _entity.ExpirationDate.Should().BeNull();
        }
    }
}
