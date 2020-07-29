using ApiAuthTokenGenerator.V1.Boundary;
using ApiAuthTokenGenerator.V1.Boundary.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.Factories
{
    public static class GenerateJwtFactory
    {
        public static GenerateJwtRequest ToJwtRequest(TokenRequestObject tokenRequestObject, int id)
        {
            return new GenerateJwtRequest
            {
                ConsumerName = tokenRequestObject.Consumer,
                ConsumerType = tokenRequestObject.ConsumerType,
                ExpiresAt = tokenRequestObject.ExpiresAt,
                Id = id
            };
        }
    }
}
