using Amazon.Lambda.Core;
using ApiAuthVerifyToken.V1.Boundary;
using ApiAuthVerifyToken.V1.Domain;
using ApiAuthVerifyToken.V1.Gateways;
using ApiAuthVerifyToken.V1.Helpers;
using ApiAuthVerifyToken.V1.UseCase.Interfaces;
using System;
using System.Linq;

namespace ApiAuthVerifyToken.V1.UseCase
{
    public class VerifyAccessUseCase : IVerifyAccessUseCase
    {
        private IAuthTokenDatabaseGateway _databaseGateway;
        private IDynamoDbGateway _dynamoDbGateway;
        public VerifyAccessUseCase(IAuthTokenDatabaseGateway databaseGateway, IDynamoDbGateway dynamoDbGateway)
        {
            _databaseGateway = databaseGateway;
            _dynamoDbGateway = dynamoDbGateway;
        }
        public AccessDetails ExecuteServiceAuth(AuthorizerRequest authorizerRequest)
        {
            LambdaLogger.Log("Begins service auth flow");

            var validTokenClaims = ValidateTokenHelper.ValidateToken(authorizerRequest.Token, Environment.GetEnvironmentVariable("jwtSecret"));
            if (validTokenClaims == null || validTokenClaims.Count == 0) return ReturnNotAuthorised(authorizerRequest);

            var tokenId = validTokenClaims.Find(x => x.Type == "id").Value;
            if (!int.TryParse(tokenId, out int id)) return ReturnNotAuthorised(authorizerRequest);

            var tokenData = _databaseGateway.GetTokenData(id);
            var apiName = tokenData.ApiName;
            LambdaLogger.Log($"API name - {apiName}");
            var allow = VerifyAccessHelper.ShouldHaveAccessServiceFlow(authorizerRequest, tokenData, apiName);
            return new AccessDetails
            {
                Allow = allow,
                User = $"{tokenData.ConsumerName}{tokenData.Id}"
            };
        }

        public AccessDetails ExecuteUserAuth(AuthorizerRequest authorizerRequest)
        {
            LambdaLogger.Log("Begins user auth flow");
            var validTokenClaims = ValidateTokenHelper.ValidateToken(authorizerRequest.Token, Environment.GetEnvironmentVariable("hackneyUserAuthTokenJwtSecret"));
            if (validTokenClaims == null || validTokenClaims.Count == 0) return ReturnNotAuthorised(authorizerRequest);

            var user = new HackneyUser();
            user.Groups = validTokenClaims.Where(x => x.Type == "groups").Select(y => y.Value).ToList();
            user.Email = validTokenClaims.Find(x => x.Type == "email").Value;

            var apiDataInDb = _dynamoDbGateway.GetAPIDataByApiIdAsync(authorizerRequest.ApiAwsId);
            var apiName = apiDataInDb.ApiName;
            LambdaLogger.Log($"API name retrieved for id {authorizerRequest.ApiAwsId} - {apiName}");
            return new AccessDetails
            {
                Allow = VerifyAccessHelper.ShouldHaveAccessUserFlow(user, authorizerRequest, apiDataInDb, apiName),
                User = validTokenClaims.Find(x => x.Type == "email").Value
            };
        }
        private static AccessDetails ReturnNotAuthorised(AuthorizerRequest authorizerRequest)
        {
            LambdaLogger.Log(
                $"Token beginning with {authorizerRequest.Token.Substring(0, 8)} is invalid or should not have access to" +
                $" {authorizerRequest.ApiAwsId} - {authorizerRequest.ApiEndpointName}" +
                $" in {authorizerRequest.Environment}");
            return new AccessDetails { Allow = false, User = "user" };
        }
    }

    public class AccessDetails
    {
        public bool Allow { get; set; }
        public string User { get; set; }
    }
}
