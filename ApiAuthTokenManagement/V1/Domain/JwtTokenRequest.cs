using System;

namespace ApiAuthTokenManagement.V1.Domain
{
    public class JwtTokenRequest
    {
        public int Id { get; set; }
        public int ConsumerType { get; set; }
        public string ConsumerName { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
