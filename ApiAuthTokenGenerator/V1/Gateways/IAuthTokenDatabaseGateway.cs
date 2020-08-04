using System.Collections.Generic;
using ApiAuthTokenGenerator.V1.Boundary.Request;
using ApiAuthTokenGenerator.V1.Domain;

namespace ApiAuthTokenGenerator.V1.Gateways
{
    public interface IAuthTokenDatabaseGateway
    {
        int GenerateToken(TokenRequestObject tokenRequestObject);
    }
}
