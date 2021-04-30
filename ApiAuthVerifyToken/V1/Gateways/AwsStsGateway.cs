using Amazon.APIGateway;
using Amazon.Lambda.Core;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApiAuthVerifyToken.V1.Gateways
{
    public class AwsStsGateway : IAwsStsGateway
    {
        public AssumeRoleResponse GetTemporaryCredentials(string awsAccount)
        {
            try
            {
                using (var stsClient = new AmazonSecurityTokenServiceClient())
                {
                    LambdaLogger.Log($"Begin assume role for AWS account with ID {awsAccount}");
                    AssumeRoleRequest request = new AssumeRoleRequest
                    {
                        RoleArn = $"arn:aws:iam::{awsAccount}:role/{Environment.GetEnvironmentVariable("AWS_ROLE_NAME_FOR_STS_API_GATEWAY_GET")}",
                        DurationSeconds = 900, //15 mins, which is the minimum accepted
                        RoleSessionName = "Session-for-retrieving-API-name"
                    };
                    var credentialsResponse = stsClient.AssumeRoleAsync(request).ConfigureAwait(true).GetAwaiter().GetResult();
                    LambdaLogger.Log($"Credentials assumed for Role ARN: {request.RoleArn}");

                    return credentialsResponse;
                }
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"An error occurred while assuming role for AWS account with ID {awsAccount}. Message: {ex.Message}");
                throw;
            }
        }
    }
}
