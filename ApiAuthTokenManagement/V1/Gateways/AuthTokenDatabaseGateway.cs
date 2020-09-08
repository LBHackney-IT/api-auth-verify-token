using System;
using System.Globalization;
using System.Linq;
using ApiAuthTokenManagement.V1.Boundary.Request;
using ApiAuthTokenManagement.V1.Domain;
using ApiAuthTokenManagement.V1.Domain.Exceptions;
using ApiAuthTokenManagement.V1.Factories;
using ApiAuthTokenManagement.V1.Infrastructure;

namespace ApiAuthTokenManagement.V1.Gateways
{
    public class AuthTokenDatabaseGateway : IAuthTokenDatabaseGateway
    {
        private readonly TokenDatabaseContext _databaseContext;

        public AuthTokenDatabaseGateway(TokenDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public int GenerateToken(TokenRequestObject tokenRequestObject)
        {
            var tokenToInsert = new AuthTokens
            {
                ApiEndpointNameLookupId = tokenRequestObject.ApiEndpoint,
                ApiLookupId = tokenRequestObject.ApiName,
                HttpMethodType = tokenRequestObject.HttpMethodType.ToUpper(CultureInfo.InvariantCulture),
                ConsumerName = tokenRequestObject.Consumer,
                ConsumerTypeLookupId = tokenRequestObject.ConsumerType,
                Environment = tokenRequestObject.Environment,
                AuthorizedBy = tokenRequestObject.AuthorizedBy,
                RequestedBy = tokenRequestObject.RequestedBy,
                DateCreated = DateTime.Now,
                ExpirationDate = tokenRequestObject.ExpiresAt,
                Enabled = true
            };

            _databaseContext.Tokens.Add(tokenToInsert);
            _databaseContext.SaveChanges();

            return tokenToInsert.Id;
        }

        public AuthToken GetTokenData(int tokenId)
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
