using ApiAuthTokenGenerator.V1.Boundary.Response;
using ApiAuthTokenGenerator.V1.Factories;
using ApiAuthTokenGenerator.V1.Gateways;
using ApiAuthTokenGenerator.V1.UseCase.Interfaces;

namespace ApiAuthTokenGenerator.V1.UseCase
{
    //TODO: Rename class name and interface name to reflect the entity they are representing eg. GetClaimantByIdUseCase
    public class GetByIdUseCase : IGetByIdUseCase
    {
        private IAuthTokenDatabaseGateway _gateway;
        public GetByIdUseCase(IAuthTokenDatabaseGateway gateway)
        {
            _gateway = gateway;
        }

        public ResponseObject Execute(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}
