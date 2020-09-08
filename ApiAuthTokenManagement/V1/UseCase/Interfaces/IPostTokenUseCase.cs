using ApiAuthTokenManagement.V1.Boundary.Request;
using ApiAuthTokenManagement.V1.Boundary.Response;

namespace ApiAuthTokenManagement.V1.UseCase.Interfaces
{
    public interface IPostTokenUseCase
    {
        GenerateTokenResponse Execute(TokenRequestObject tokenRequest);
    }
}
