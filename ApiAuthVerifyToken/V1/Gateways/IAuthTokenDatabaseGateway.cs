using ApiAuthVerifyToken.V1.Domain;

namespace ApiAuthVerifyToken.V1.Gateways
{
    public interface IAuthTokenDatabaseGateway
    {
        AuthTokenServiceFlow GetTokenData(int tokenId);
    }
}
