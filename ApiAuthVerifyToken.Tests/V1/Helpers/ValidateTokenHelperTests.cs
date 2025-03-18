using ApiAuthVerifyToken.V1.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace ApiAuthVerifyToken.Tests.V1.Helpers
{
    public class ValidateTokenHelperTests
    {
        [Test]
        public void Test()
        {
            // Arrange
            var token = "";
            var secret = "";

            // Act
            var result = ValidateTokenHelper.ValidateToken(token, secret);

            // Assert
            result.Should().NotBeNull();
        }
    }
}
