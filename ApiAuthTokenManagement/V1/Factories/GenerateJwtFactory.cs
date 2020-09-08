using ApiAuthTokenManagement.V1.Boundary.Request;
using ApiAuthTokenManagement.V1.Domain;

namespace ApiAuthTokenManagement.V1.Factories
{
    public static class GenerateJwtFactory
    {
        public static JwtTokenRequest ToJwtRequest(TokenRequestObject tokenRequestObject, int id)
        {
            return new JwtTokenRequest
            {
                ConsumerName = tokenRequestObject.Consumer,
                ConsumerType = tokenRequestObject.ConsumerType,
                ExpiresAt = tokenRequestObject.ExpiresAt,
                Id = id
            };
        }
    }
}
