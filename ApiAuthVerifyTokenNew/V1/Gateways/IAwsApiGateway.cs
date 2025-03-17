using Amazon.SecurityToken.Model;

namespace ApiAuthVerifyTokenNew.V1.Gateways
{
    public interface IAwsApiGateway
    {
        string GetApiName(string apiId, Credentials awsCredentials);
    }
}
