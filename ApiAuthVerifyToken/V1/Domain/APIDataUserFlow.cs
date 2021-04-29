using System;
using System.Collections.Generic;
using System.Text;

namespace ApiAuthVerifyToken.V1.Domain
{
    public class APIDataUserFlow
    {
        public Guid Id { get; set; }
        public string ApiName { get; set; }
        public string Environemnt { get; set; }
        //AWS account where API is deployed
        public string AwsAccount { get; set; }
        public List<string> AllowedGroups { get; set; }
    }
}
