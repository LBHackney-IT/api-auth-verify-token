using ApiAuthTokenManagement.V1.Boundary.Request;
using ApiAuthTokenManagement.V1.Domain;

namespace ApiAuthTokenManagement.V1.Gateways
{
    public interface IAuthTokenDatabaseGateway
    {
        int GenerateToken(TokenRequestObject tokenRequestObject);
        AuthToken GetTokenData(int tokenId);
    }
}
