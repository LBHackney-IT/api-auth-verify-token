namespace ApiAuthVerifyToken.V1.Gateways
{
    public interface IAwsApiGateway
    {
        string GetApiName(string apiId);
    }
}
