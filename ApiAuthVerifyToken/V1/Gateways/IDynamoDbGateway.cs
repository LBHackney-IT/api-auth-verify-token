using ApiAuthVerifyToken.V1.Domain;

namespace ApiAuthVerifyToken.V1.Gateways
{
    public interface IDynamoDbGateway
    {
        APIDataUserFlow GetAPIDataByNameAndEnvironmentAsync(string apiName, string environment);
        APIDataUserFlow GetAPIDataByApiGatewayIdAsync(string apiAwsId);
    }
}
