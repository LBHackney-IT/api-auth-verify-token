using Amazon.SecurityToken.Model;

namespace ApiAuthVerifyToken.V1.Gateways
{
    public interface IAwsApiGateway
    {
        string GetApiName(string apiId, Credentials awsCredentials);
    }
}
