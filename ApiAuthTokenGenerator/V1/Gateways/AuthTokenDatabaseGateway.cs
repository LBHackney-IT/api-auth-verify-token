using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
                Valid = true
            };

            _databaseContext.Tokens.Add(tokenToInsert);
            _databaseContext.SaveChanges();

            return tokenToInsert.Id;
        }
    }
}