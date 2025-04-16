using ApiAuthVerifyToken.V1.Infrastructure;
using AutoFixture;

namespace ApiAuthVerifyToken.Tests.V1.TestHelper
{
    public static class AuthTokenDatabaseEntityHelper
    {
        public static AuthTokens CreateDatabaseEntity()
        {
            var entity = new Fixture().Create<AuthTokens>();

            return CreateDatabaseEntityFrom(entity);
        }

        public static AuthTokens CreateDatabaseEntityFrom(AuthTokens entity)
        {
            return new AuthTokens
            {
                Id = entity.Id,
                ApiEndpointNameLookupId = entity.ApiEndpointNameLookupId,
                ApiLookupId = entity.ApiLookupId,
                AuthorizedBy = entity.AuthorizedBy,
                ConsumerName = entity.ConsumerName,
                ConsumerTypeLookupId = entity.ConsumerTypeLookupId,
                DateCreated = entity.DateCreated,
                Environment = entity.Environment,
                HttpMethodType = entity.HttpMethodType,
                ExpirationDate = entity.ExpirationDate,
                RequestedBy = entity.RequestedBy,
                Enabled = entity.Enabled
            };
        }
    }
}
