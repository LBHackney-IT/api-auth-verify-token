using ApiAuthVerifyToken.V1.Domain;

namespace ApiAuthVerifyToken.V1.Gateways
{
    public interface IAuthTokenDatabaseGateway
    {
        AuthToken GetTokenData(int tokenId);
    }
}
