using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.Gateways
{
    public interface IAwsApiGateway
    {
        string GetApiName(string apiId);
    }
}
