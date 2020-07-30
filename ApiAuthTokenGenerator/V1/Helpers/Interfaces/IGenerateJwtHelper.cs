using ApiAuthTokenGenerator.V1.Boundary;
using ApiAuthTokenGenerator.V1.Boundary.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.Helpers.Interfaces
{
    public interface IGenerateJwtHelper
    {
        string GenerateJwtToken(GenerateJwtRequest tokenRequestObject);
    }
}
