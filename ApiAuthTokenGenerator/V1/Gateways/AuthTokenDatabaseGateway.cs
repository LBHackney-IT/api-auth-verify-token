using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Amazon.Lambda.Core;
using ApiAuthTokenGenerator.V1.Boundary.Request;
using ApiAuthTokenGenerator.V1.Domain;
using ApiAuthTokenGenerator.V1.Factories;
using ApiAuthTokenGenerator.V1.Infrastructure;

namespace ApiAuthTokenGenerator.V1.Gateways
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
            var token = _databaseContext.Tokens.Where(x => x.Id == tokenId).FirstOrDefault();

            if (token == null)
            {
                throw new TokenDataNotFoundException(); //No match is found
            }
            var endpointName = _databaseContext.ApiEndpointNameLookups.Where(x => x.Id == token.ApiEndpointNameLookupId)
                .FirstOrDefault();
            var apiName = _databaseContext.ApiNameLookups.Where(x => x.Id == token.ApiLookupId).FirstOrDefault();
            var consumerType = _databaseContext.ConsumerTypeLookups.Where(x => x.Id == token.ConsumerTypeLookupId)
                .FirstOrDefault();

            LambdaLogger.Log("api: " + apiName.ApiName);

            return token.ToDomain(endpointName.ApiEndpointName, apiName.ApiName, consumerType.TypeName);
        }
    }
}
