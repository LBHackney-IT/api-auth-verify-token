using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthVerifyToken.V1.Domain
{
    public class AuthTokenServiceFlow
    {
        public int Id { get; set; }
        public string ApiName { get; set; }
        public string ApiEndpointName { get; set; }
        [MaxLength(6)]
        public string HttpMethodType { get; set; }
        public string Environment { get; set; }
        public string ConsumerName { get; set; }
        public string ConsumerType { get; set; }
        public bool Enabled { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
