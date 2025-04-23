using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.Lambda.Core;
using Amazon.SecurityToken.Model;
using ApiAuthVerifyToken.V1.Domain;
using System;

namespace ApiAuthVerifyToken.V1.Gateways
{
    public class AwsApiGateway : IAwsApiGateway
    {
        private AmazonAPIGatewayClient _client;
        public string GetApiName(string apiId, Credentials awsCredentials)
        {
            try
            {
                using (_client = new AmazonAPIGatewayClient(awsCredentials))
                {
                    LambdaLogger.Log($"Begin getting API name for API id {apiId}");
                    var response = _client.GetRestApiAsync(new GetRestApiRequest { RestApiId = apiId }).Result;

                    if (response == null)
                    {
                        throw new AwsApiNotFoundException();
                    }
                    return response.Name;
                }
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"An error occurred while getting API name for API id {apiId}. Message: {ex.Message}, Exception: {ex.InnerException}");
                throw;
            }
        }
    }
}
