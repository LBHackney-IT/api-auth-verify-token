using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ApiAuthVerifyToken.V1.Helpers;
using ApiAuthVerifyToken.V1.UseCase.Interfaces;
using Microsoft.Extensions.DependencyInjection;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ApiAuthVerifyToken.V1.Boundary
{
    public class VerifyTokenHandler
    {
        private readonly IServiceProvider _serviceProvider;

        //Initialise services
        public VerifyTokenHandler()
        {
            var services = new ServiceCollection();
            services.Configure();
            _serviceProvider = services.BuildServiceProvider();
        }

        public VerifyTokenHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public APIGatewayCustomAuthorizerResponse VerifyToken(APIGatewayCustomAuthorizerRequest request)
        {
            LambdaLogger.Log("domain:" + request.RequestContext.DomainName);
            LambdaLogger.Log("rId:" + request.RequestContext.ResourceId);
            LambdaLogger.Log("path: " + request.RequestContext.Path);
            LambdaLogger.Log("stage: " + request.RequestContext.Stage);
            LambdaLogger.Log("key: " + request.RequestContext.RouteKey);
            try
            {              
                var authorizerRequest = new AuthorizerRequest
                {
                    ApiEndpointName = request.RequestContext.Path,
                    ApiAwsId = request.RequestContext.ApiId,
                    Environment = request.RequestContext.Stage,
                    HttpMethodType = request.RequestContext.HttpMethod,
                    AwsAccountId = request.RequestContext.AccountId,
                    Token = request.Headers["Authorization"]?.Replace("Bearer ", "")
                };

                var isServiceAuthFlow = DetermineTokenSourceHelper.DetermineTokenSource(authorizerRequest.Token);

                var verifyAccessUseCase = _serviceProvider.GetService<IVerifyAccessUseCase>();
                LambdaLogger.Log($"Executing service auth flow? -> {isServiceAuthFlow}");

                var result = isServiceAuthFlow ?
                            verifyAccessUseCase.ExecuteServiceAuth(authorizerRequest) : verifyAccessUseCase.ExecuteUserAuth(authorizerRequest);

                return new APIGatewayCustomAuthorizerResponse
                {
                    PrincipalID = result.User,
                    PolicyDocument = new APIGatewayCustomAuthorizerPolicy
                    {
                        Version = "2012-10-17",
                        Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>()
                        {
                          new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                          {
                               Action = new HashSet<string> {"execute-api:Invoke"},
                               Effect = result.Allow ? "Allow" : "Deny",
                               Resource = new HashSet<string>(){  request.MethodArn } // resource arn here
                          }
                        }
                    },
                };
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Verify token in catch:" + e.Message);
                return new APIGatewayCustomAuthorizerResponse
                {
                    PolicyDocument = new APIGatewayCustomAuthorizerPolicy
                    {
                        Version = "2012-10-17",
                        Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>()
                        {
                             new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                             {
                                 Action = new HashSet<string>(){"execute-api:Invoke"},
                                 Effect = "Deny",
                                 Resource = new HashSet<string>(){  request.MethodArn } // resource arn here
                             }
                        }
                    },
                };
            }

        }
    }
}
