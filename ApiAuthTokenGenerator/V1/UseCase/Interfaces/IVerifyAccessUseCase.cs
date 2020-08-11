using ApiAuthTokenGenerator.V1.Boundary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.UseCase.Interfaces
{
    public interface IVerifyAccessUseCase
    {
        bool Execute(AuthorizerRequest authorizerRequest);
    }
}
