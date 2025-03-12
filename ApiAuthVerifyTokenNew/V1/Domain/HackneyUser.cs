using System;
using System.Collections.Generic;
using System.Text;

namespace ApiAuthVerifyToken.V1.Domain
{
    public class HackneyUser
    {
        public string Email { get; set; }
        public List<string> Groups { get; set; }
    }
}
