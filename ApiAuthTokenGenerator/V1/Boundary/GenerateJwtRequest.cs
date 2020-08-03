using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.Boundary
{
    public class GenerateJwtRequest
    {
        public int Id { get; set; }
        public int ConsumerType { get; set; }
        public string ConsumerName { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
