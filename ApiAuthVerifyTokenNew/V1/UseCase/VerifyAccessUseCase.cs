using Amazon.Lambda.Core;
using ApiAuthVerifyTokenNew.V1.Boundary;
using ApiAuthVerifyTokenNew.V1.Domain;
using ApiAuthVerifyTokenNew.V1.Gateways;
using ApiAuthVerifyTokenNew.V1.Helpers;
using ApiAuthVerifyTokenNew.V1.UseCase.Interfaces;
using System;
using System.Linq;

namespace ApiAuthVerifyTokenNew.V1.UseCase
{
    public class VerifyAccessUseCase : IVerifyAccessUseCase
    {
        private IAuthTokenDatabaseGateway _databaseGateway;
        private IAwsApiGateway _awsApiGateway;
        private IAwsStsGateway _awsStsGateway;
        private IDynamoDbGateway _dynamoDbGateway;
        public VerifyAccessUseCase(IAuthTokenDatabaseGateway databaseGateway, IAwsApiGateway awsApiGateway, IAwsStsGateway stsGateway, IDynamoDbGateway dynamoDbGateway)
        {
            _databaseGateway = databaseGateway;
            _awsApiGateway = awsApiGateway;
            _awsStsGateway = stsGateway;
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
            var credentials = _awsStsGateway.GetTemporaryCredentials(authorizerRequest.AwsAccountId).Credentials;
            var apiName = _awsApiGateway.GetApiName(authorizerRequest.ApiAwsId, credentials);
            LambdaLogger.Log($"API name retrieved - {apiName}");
            return new AccessDetails
            {
                Allow = VerifyAccessHelper.ShouldHaveAccessServiceFlow(authorizerRequest, tokenData, apiName),
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

            //get STS credentials and pass them to API gateway
            var credentials = _awsStsGateway.GetTemporaryCredentials(authorizerRequest.AwsAccountId).Credentials;
            //get API name
            var apiName = _awsApiGateway.GetApiName(authorizerRequest.ApiAwsId, credentials);
            LambdaLogger.Log($"API name retrieved - {apiName}");
            //check if API is in the DynamoDB
            var apiDataInDb = _dynamoDbGateway.GetAPIDataByNameAndEnvironmentAsync(apiName, authorizerRequest.Environment);
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
