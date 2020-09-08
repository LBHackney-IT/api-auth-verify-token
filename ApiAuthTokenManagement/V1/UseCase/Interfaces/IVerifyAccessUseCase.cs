using ApiAuthTokenManagement.V1.Boundary.Request;

namespace ApiAuthTokenManagement.V1.UseCase.Interfaces
{
    public interface IVerifyAccessUseCase
    {
        AccessDetails Execute(AuthorizerRequest authorizerRequest);
    }
}
