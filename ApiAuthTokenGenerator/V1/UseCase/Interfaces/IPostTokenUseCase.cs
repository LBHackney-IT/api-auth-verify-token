using ApiAuthTokenGenerator.V1.Boundary.Request;
using ApiAuthTokenGenerator.V1.Boundary.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.UseCase.Interfaces
{
    public interface IPostTokenUseCase
    {
        GenerateTokenResponse Execute(TokenRequestObject tokenRequest);
    }
}
