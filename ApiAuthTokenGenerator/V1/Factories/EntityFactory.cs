using ApiAuthTokenGenerator.V1.Domain;
using ApiAuthTokenGenerator.V1.Infrastructure;
using System;

namespace ApiAuthTokenGenerator.V1.Factories
{
    public static class EntityFactory
    {
        public static AuthToken ToDomain(this AuthTokens token, string apiEndpointName, string apiName,
            string consumerType)
        {
            return new AuthToken
            {
                Id = token.Id,
                ApiEndpointName = apiEndpointName,
                ApiName = apiName,
                ConsumerName = token.ConsumerName,
                ConsumerType = consumerType,
                Environment = token.Environment,
                ExpirationDate = token.ExpirationDate,
                Valid = token.Valid
            };
        }
    }
}
