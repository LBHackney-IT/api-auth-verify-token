using ApiAuthTokenGenerator.V1.Boundary.Response;

namespace ApiAuthTokenGenerator.V1.UseCase.Interfaces
{
    public interface IGetAllUseCase
    {
        ResponseObjectList Execute();
    }
}
