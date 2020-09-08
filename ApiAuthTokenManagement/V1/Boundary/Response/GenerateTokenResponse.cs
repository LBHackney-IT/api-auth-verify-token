using System;

namespace ApiAuthTokenManagement.V1.Boundary.Response
{
    public class GenerateTokenResponse
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
