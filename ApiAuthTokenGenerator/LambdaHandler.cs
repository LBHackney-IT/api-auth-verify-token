using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ApiAuthTokenGenerator.V1.Boundary;
using ApiAuthTokenGenerator.V1.UseCase.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ApiAuthTokenGenerator
{
    public class LambdaHandler
    {
        private readonly IServiceProvider _serviceProvider;

        //Initialise services
        public LambdaHandler()
        {
            var services = new ServiceCollection();
            services.Configure();
            _serviceProvider = services.BuildServiceProvider();
        }

        public LambdaHandler(IServiceProvider serviceProvider)
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
            LambdaLogger.Log("key: " + request.RequestContext.ToString());
            try
            {
                var authorizerRequest = new AuthorizerRequest
                {
                    ApiEndpointName = request.RequestContext.Path,
                    ApiAwsId = request.RequestContext.ApiId,
                    Environment = request.RequestContext.Stage,
                    HttpMethodType = request.RequestContext.HttpMethod,
                    Token = request.Headers["Authorization"]
                };
                var verifyAccessUseCase = _serviceProvider.GetService<IVerifyAccessUseCase>();

                var result = verifyAccessUseCase.Execute(authorizerRequest);

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
                               Effect = result ? "Allow" : "Deny",
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
