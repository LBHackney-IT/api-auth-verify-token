using Amazon.APIGateway.Model;
using ApiAuthTokenGenerator.V1.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.Gateways
{
    public class AwsApiGateway : IAwsApiGateway
    {
        private HttpClient _httpClient;
        public AwsApiGateway()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri($"http://apigateway.eu-west-2.amazonaws.com");

        }
        public string GetApiName(string apiId)
        {
            var response = _httpClient.GetAsync(new Uri($"/restapis/{apiId}")).Result;
            if (response == null)
            {
                throw new AwsApiNotFoundException();
            }
            GetRestApiResponse apiResponse = JsonConvert.DeserializeObject<GetRestApiResponse>(response.Content.ToString());
            return apiResponse.Name;
        }
    }
}
