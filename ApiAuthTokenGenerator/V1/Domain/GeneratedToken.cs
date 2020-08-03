using System;

namespace ApiAuthTokenGenerator.V1.Domain
{
    public class GeneratedToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
