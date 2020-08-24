using Amazon.APIGateway.Model;
using Amazon.Lambda.Core;
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
            _httpClient.BaseAddress = new Uri(@"https://apigateway.eu-west-2.amazonaws.com");

        }
        public string GetApiName(string apiId)
        {
            var response = _httpClient.GetAsync(new Uri($@"/restapis/{apiId}", UriKind.Relative)).Result;
            if (response == null)
            {
                throw new AwsApiNotFoundException();
            }
            var responseData = response.Content.ReadAsStringAsync().Result;

            GetRestApiResponse apiResponse = JsonConvert.DeserializeObject<GetRestApiResponse>(responseData);
            return apiResponse.Name;
        }
    }
}
