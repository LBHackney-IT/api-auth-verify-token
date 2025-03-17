using ApiAuthVerifyToken.V1.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApiAuthVerifyTokenNew.V1.Gateways
{
    public interface IDynamoDbGateway
    {
        APIDataUserFlow GetAPIDataByNameAndEnvironmentAsync(string apiName, string environment);
    }
}
