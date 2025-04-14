using System;

namespace ApiAuthVerifyToken.V1.Domain
{
    public class TokenDataNotFoundException : Exception
    {
        public TokenDataNotFoundException()
        {
        }

        public TokenDataNotFoundException(string message) : base(message)
        {
        }
    }
}
