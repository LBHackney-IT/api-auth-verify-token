using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiAuthVerifyToken.V1.Infrastructure
{
    [DynamoDBTable("APIAuthenticatorData", LowerCamelCaseProperties = true)]
    public class APIDataUserFlowDbEntity
    {
        [DynamoDBHashKey]
        public string ApiName { get; set; }
        [DynamoDBRangeKey("environment")]
        public string Environment { get; set; }
        //AWS account where API is deployed
        public string AwsAccount { get; set; }
        [DynamoDBGlobalSecondaryIndexHashKey("apiGatewayIdIndex")]
        public string ApiGatewayId { get; set; }
        public List<string> AllowedGroups { get; set; }
    }
}
