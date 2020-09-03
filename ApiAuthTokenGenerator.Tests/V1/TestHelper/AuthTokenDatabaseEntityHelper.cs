using AutoFixture;
using ApiAuthTokenGenerator.V1.Domain;
using ApiAuthTokenGenerator.V1.Infrastructure;

namespace ApiAuthTokenGenerator.Tests.V1.Helper
{
    public static class AuthTokenDatabaseEntityHelper
    {
        public static ApiAuthTokenGenerator.V1.Infrastructure.AuthTokens CreateDatabaseEntity()
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
