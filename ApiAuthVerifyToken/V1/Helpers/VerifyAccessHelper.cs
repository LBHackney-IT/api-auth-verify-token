using Amazon.Lambda.Core;
using System;
using ApiAuthVerifyToken.V1.Boundary;
using ApiAuthVerifyToken.V1.Domain;
using System.Linq;
using Newtonsoft.Json;

namespace ApiAuthVerifyToken.V1.Helpers
{
    public static class VerifyAccessHelper
    {
        public static bool ShouldHaveAccessServiceFlow(AuthorizerRequest authorizerRequest, AuthTokenServiceFlow tokenData, string apiName)
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

        public static bool ShouldHaveAccessUserFlow(HackneyUser user, AuthorizerRequest authorizerRequest, APIDataUserFlow apiData, string apiName)
        {
            bool groupIsAllowed = apiData.AllowedGroups.Any(x => user.Groups.Contains(x));

            if (!groupIsAllowed
                 || apiData.ApiName != apiName
                 || apiData.Environment != authorizerRequest.Environment
                 || apiData.AwsAccount != authorizerRequest.AwsAccountId)
            {
                LambdaLogger.Log($"User with email {user.Email} is DENIED access for {apiName} " +
                  $" in {authorizerRequest.Environment} stage. User does not have access to {apiName} " +
                  $"for {apiData.Environment} stage in the following AWS account {apiData.AwsAccount}. User is in the following" +
                  $"Google groups: {user.Groups}");
                return false;
            }

            LambdaLogger.Log($"User with email {user.Email} is ALLOWED access for {apiName} " +
                  $" in {authorizerRequest.Environment} stage. The API, as described in the database," +
                  $"is deployed to the following AWS account {apiData.AwsAccount}");

            return true;
        }

    }
}
