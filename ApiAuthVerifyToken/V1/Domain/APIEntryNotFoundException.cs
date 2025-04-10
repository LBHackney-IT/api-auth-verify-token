using System;
using System.Collections.Generic;
using System.Text;

namespace ApiAuthVerifyToken.V1.Domain
{
    public class APIEntryNotFoundException : Exception
    {
        public APIEntryNotFoundException() : base() { }

        public APIEntryNotFoundException(string message) : base(message) { }

        public APIEntryNotFoundException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
