using ApiAuthTokenGenerator.V1.Boundary.Response;
using ApiAuthTokenGenerator.V1.Factories;
using ApiAuthTokenGenerator.V1.Gateways;
using ApiAuthTokenGenerator.V1.UseCase.Interfaces;
using System;

namespace ApiAuthTokenGenerator.V1.UseCase
{
    //TODO: Rename class name and interface name to reflect the entity they are representing eg. GetAllClaimantsUseCase
    public class GetAllUseCase : IGetAllUseCase
    {
        private readonly IAuthTokenDatabaseGateway _gateway;
        public GetAllUseCase(IAuthTokenDatabaseGateway gateway)
        {
            _gateway = gateway;
        }

        public ResponseObjectList Execute()
        {
            throw new NotImplementedException();
        }
    }
}
