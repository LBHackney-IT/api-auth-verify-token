using ApiAuthTokenGenerator.V1.Boundary.Response;

namespace ApiAuthTokenGenerator.V1.UseCase.Interfaces
{
    public interface IGetByIdUseCase
    {
        ResponseObject Execute(int id);
    }
}
