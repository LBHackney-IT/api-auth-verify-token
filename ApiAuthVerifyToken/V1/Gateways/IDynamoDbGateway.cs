using ApiAuthVerifyToken.V1.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApiAuthVerifyToken.V1.Gateways
{
    public interface IDynamoDbGateway
    {
        APIDataUserFlow GetAPIDataByNameAndEnvironmentAsync(string apiName, string environment);
        APIDataUserFlow GetAPIDataByApiIdAsync(string apiAwsId);
    }
}
