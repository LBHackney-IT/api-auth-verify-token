using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using ApiAuthTokenManagement.V1.Domain.Exceptions;

namespace ApiAuthTokenManagement.V1.Gateways
{
    public class AwsApiGateway : IAwsApiGateway
    {
        private AmazonAPIGatewayClient _client;
        public string GetApiName(string apiId)
        {
            using (_client = new AmazonAPIGatewayClient())
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
