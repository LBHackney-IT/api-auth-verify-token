using System;
using System.Collections.Generic;
using System.Text;

namespace ApiAuthVerifyTokenNew.V1.Domain
{
    public class HackneyUser
    {
        public string Email { get; set; }
        public List<string> Groups { get; set; }
    }
}
