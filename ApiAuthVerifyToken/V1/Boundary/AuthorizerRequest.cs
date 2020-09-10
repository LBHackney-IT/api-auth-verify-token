using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthVerifyToken.V1.Boundary
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
