using Amazon.Lambda.Core;
using ApiAuthTokenGenerator.V1.Boundary;
using ApiAuthTokenGenerator.V1.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.Helpers
{
    public static class VerifyAccessHelper
    {
        public static bool ShouldHaveAccess(AuthorizerRequest authorizerRequest, AuthToken tokenData, string apiName)
        {
            if (!tokenData.Enabled
                || (tokenData.ExpirationDate != null && tokenData.ExpirationDate < DateTime.Now)
                || tokenData.Environment != authorizerRequest.Environment
                || tokenData.ApiEndpointName != authorizerRequest.ApiEndpointName
                || tokenData.ApiName != apiName)
            {
                LambdaLogger.Log($"Token with id {tokenData.Id} allowing access for {tokenData.ApiName} with endpoint {tokenData.ApiEndpointName}" +
                    $" in {tokenData.Environment} stage does not have access to {apiName} with endpoint {authorizerRequest.ApiEndpointName} " +
                    $"for {authorizerRequest.Environment} stage");
                return false;
            }
            return true;
        }
    }
}
