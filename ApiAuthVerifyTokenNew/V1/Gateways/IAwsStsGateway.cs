using Amazon.SecurityToken.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApiAuthVerifyToken.V1.Gateways
{
    public interface IAwsStsGateway
    {
        AssumeRoleResponse GetTemporaryCredentials(string awsAccount);
    }
}
