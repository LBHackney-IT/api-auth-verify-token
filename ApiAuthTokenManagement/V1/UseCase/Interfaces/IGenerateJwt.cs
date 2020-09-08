using ApiAuthTokenManagement.V1.Domain;

namespace ApiAuthTokenManagement.V1.UseCase.Interfaces
{
    public interface IGenerateJwt
    {
        string Execute(JwtTokenRequest jwtTokenRequestObject);
    }
}
