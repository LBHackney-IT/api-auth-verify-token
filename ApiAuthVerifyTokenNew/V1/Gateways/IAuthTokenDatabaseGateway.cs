using ApiAuthVerifyTokenNew.V1.Domain;

namespace ApiAuthVerifyTokenNew.V1.Gateways
{
    public interface IAuthTokenDatabaseGateway
    {
        AuthTokenServiceFlow GetTokenData(int tokenId);
    }
}
