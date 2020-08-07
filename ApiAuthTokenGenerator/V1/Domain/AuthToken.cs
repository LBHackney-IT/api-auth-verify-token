using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.Domain
{
    public class AuthToken
    {
        public int Id { get; set; }
        public string ApiName { get; set; }
        public string ApiEndpointName { get; set; }
        public string Environment { get; set; }
        public string ConsumerName { get; set; }
        public string ConsumerType { get; set; }
        public bool Valid { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
