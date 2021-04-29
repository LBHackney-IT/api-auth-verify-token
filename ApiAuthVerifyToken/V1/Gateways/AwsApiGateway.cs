using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.SecurityToken.Model;
using ApiAuthVerifyToken.V1.Domain;

namespace ApiAuthVerifyToken.V1.Gateways
{
    public class AwsApiGateway : IAwsApiGateway
    {
        private AmazonAPIGatewayClient _client;
        public string GetApiName(string apiId, Credentials awsCredentials)
        {
            using (_client = new AmazonAPIGatewayClient(awsCredentials))
            {
                var response = _client.GetRestApiAsync(new GetRestApiRequest { RestApiId = apiId }).Result;

                if (response == null)
                {
                    throw new AwsApiNotFoundException();
                }
                return response.Name;
            }
        }
    }
}
