using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiAuthVerifyToken.V1.Boundary;

namespace ApiAuthVerifyToken.V1.UseCase.Interfaces
{
    public interface IVerifyAccessUseCase
    {
        AccessDetails ExecuteServiceAuth(AuthorizerRequest authorizerRequest);
        AccessDetails ExecuteUserAuth(AuthorizerRequest authorizerRequest);

    }
}
