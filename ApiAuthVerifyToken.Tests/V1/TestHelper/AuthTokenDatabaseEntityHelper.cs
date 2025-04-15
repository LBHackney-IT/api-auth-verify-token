using ApiAuthVerifyToken.V1.Infrastructure;
using AutoFixture;

namespace ApiAuthVerifyToken.Tests.V1.TestHelper
{
    public static class AuthTokenDatabaseEntityHelper
    {
        public static AuthTokens CreateDatabaseEntity(TokenDatabaseContext databaseContext)
        {
            var fixture = new Fixture();

            var api = fixture.Build<ApiNameLookup>().Create();
            databaseContext.Add(api);
            databaseContext.SaveChanges();

            var apiEndpoint = fixture.Build<ApiEndpointNameLookup>()
                .With(x => x.ApiLookupId, api.Id).Create();
            databaseContext.Add(apiEndpoint);
            databaseContext.SaveChanges();

            var consumerType = fixture.Build<ConsumerTypeLookup>().Create();
            databaseContext.Add(consumerType);
            databaseContext.SaveChanges();

            var entity = fixture.Build<AuthTokens>()
                .With(x => x.ApiEndpointNameLookupId, apiEndpoint.Id)
                .With(x => x.ApiLookupId, api.Id)
                .With(x => x.ConsumerTypeLookupId, consumerType.Id)
                .Create();

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
