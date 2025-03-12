using System.Linq;
using ApiAuthVerifyToken.V1.Factories;
using ApiAuthVerifyToken.V1.Domain;
using ApiAuthVerifyToken.V1.Infrastructure;

namespace ApiAuthVerifyToken.V1.Gateways
{
    public class AuthTokenDatabaseGateway : IAuthTokenDatabaseGateway
    {
        private readonly TokenDatabaseContext _databaseContext;

        public AuthTokenDatabaseGateway(TokenDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public AuthTokenServiceFlow GetTokenData(int tokenId)
        {
            var token = _databaseContext.Tokens.Find(tokenId);  //.Where(x => x.Id == tokenId).FirstOrDefault();

            if (token == null)
            {
                throw new TokenDataNotFoundException(); //No match is found
            }
            var endpointName = _databaseContext.ApiEndpointNameLookups
                .First(x => x.Id == token.ApiEndpointNameLookupId);
            var apiName = _databaseContext.ApiNameLookups
                .First(x => x.Id == token.ApiLookupId);
            var consumerType = _databaseContext.ConsumerTypeLookups
                .First(x => x.Id == token.ConsumerTypeLookupId);

            return token.ToDomain(endpointName.ApiEndpointName, apiName.ApiName, consumerType.TypeName);
        }
    }
}
