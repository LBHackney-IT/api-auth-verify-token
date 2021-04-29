using ApiAuthVerifyToken.V1.Domain;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthVerifyToken.Tests.V1.Domain
{
    [TestFixture]
    public class APIDataUserFlowTest
    {
        private APIDataUserFlow _entity;
        [SetUp]
        public void Setup()
        {
            _entity = new APIDataUserFlow();
        }
        [Test]
        public void ApiDataHasApiName()
        {
            _entity.ApiName.Should().BeNullOrWhiteSpace();
        }
        [Test]
        public void ApiDataHasEnvironment()
        {
            _entity.Environemnt.Should().BeNullOrWhiteSpace();
        }
        [Test]
        public void ApiDataHasAWSaccount()
        {
            _entity.AwsAccount.Should().BeNullOrWhiteSpace();
        }
        [Test]
        public void ApiDataHasAllowedGroups()
        {
            _entity.AllowedGroups.Should().BeNull();
        }
    }
}
