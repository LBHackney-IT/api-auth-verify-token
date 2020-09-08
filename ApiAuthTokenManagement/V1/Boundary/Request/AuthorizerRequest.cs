namespace ApiAuthTokenManagement.V1.Boundary.Request
{
    public class AuthorizerRequest
    {
        public string Token { get; set; }
        public string Environment { get; set; }
        public string ApiAwsId { get; set; }
        public string ApiEndpointName { get; set; }
        public string HttpMethodType { get; set; }
    }
}
