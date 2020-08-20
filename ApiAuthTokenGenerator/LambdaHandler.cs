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

            LambdaLogger.Log("p1: " + request.RequestContext.ResourcePath);
            LambdaLogger.Log("p2: " + request.RequestContext.ApiId);
            LambdaLogger.Log("p3: " + request.RequestContext.ResourcePath);
            LambdaLogger.Log("p4: " + request.RequestContext.Stage);
            LambdaLogger.Log("p5: " + request.Headers["Authorisation"]);
            //Only log when not in production
            try
            {
                if (Environment.GetEnvironmentVariable("LambdaEnvironment").Equals("staging", StringComparison.OrdinalIgnoreCase))
                    LambdaLogger.Log("token is: " + request.AuthorizationToken);

                var authorizerRequest = new AuthorizerRequest
                {
                    ApiEndpointName = request.RequestContext.ResourcePath,
                    ApiAwsId = request.RequestContext.ApiId,
                    Environment = request.RequestContext.Stage,
                    Token = request.Headers["Authorisation"]
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
                LambdaLogger.Log("Verify token in catch:" + e.Message + " in catch: " + request.AuthorizationToken);
                return new APIGatewayCustomAuthorizerResponse();
            }

        }
    }
}
