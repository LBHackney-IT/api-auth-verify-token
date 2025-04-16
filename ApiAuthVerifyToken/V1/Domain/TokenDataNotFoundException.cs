using System;

namespace ApiAuthVerifyToken.V1.Domain
{
    public class TokenDataNotFoundException : Exception
    {
        public TokenDataNotFoundException() : base() { }

        public TokenDataNotFoundException(string message) : base(message) { }
    }
}
