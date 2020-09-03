using Amazon.APIGateway;
using Amazon.Runtime;

using Amazon.Lambda.Core;
using ApiAuthTokenGenerator.V1.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.APIGateway.Model;

namespace ApiAuthTokenGenerator.V1.Gateways
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
