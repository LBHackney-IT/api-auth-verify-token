using Amazon.Lambda.Core;
using System;
using ApiAuthVerifyToken.V1.Boundary;
using ApiAuthVerifyToken.V1.Domain;


namespace ApiAuthVerifyToken.V1.Helpers
{
    public static class VerifyAccessHelper
    {
        public static bool ShouldHaveAccess(AuthorizerRequest authorizerRequest, AuthToken tokenData, string apiName)
        {
            //Check that the token is enabled or that the expiration date is valid
            if (!tokenData.Enabled
                || (tokenData.ExpirationDate != null && tokenData.ExpirationDate < DateTime.Now)
                || tokenData.ApiName != apiName
                || tokenData.HttpMethodType != authorizerRequest.HttpMethodType
                || tokenData.Environment != authorizerRequest.Environment
                || !authorizerRequest.ApiEndpointName.Contains(tokenData.ApiEndpointName, StringComparison.InvariantCulture))
            {
                LambdaLogger.Log($"Token with id {tokenData.Id} denying access for {tokenData.ApiName} with endpoint {tokenData.ApiEndpointName}" +
                   $" in {tokenData.Environment} stage does not have access to {apiName} with endpoint {authorizerRequest.ApiEndpointName} " +
                   $"for {authorizerRequest.Environment} stage { tokenData.Enabled }");
                return false;
            }

            LambdaLogger.Log($"Token with id {tokenData.Id} allowing access for {tokenData.ApiName} with endpoint {tokenData.ApiEndpointName}" +
                   $" in {tokenData.Environment} stage has access to {apiName} with endpoint {authorizerRequest.ApiEndpointName} " +
                   $"for {authorizerRequest.Environment} stage { tokenData.Enabled }");

            return true;
        }
    }
}
