using ApiAuthTokenGenerator.V1.Boundary.Request;
using ApiAuthTokenGenerator.V1.Boundary.Response;
using ApiAuthTokenGenerator.V1.Factories;
using ApiAuthTokenGenerator.V1.Helpers.Interfaces;
using ApiAuthTokenGenerator.V1.UseCase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.UseCase
{
    public class PostTokenUseCase : IPostTokenUseCase
    {
        private IGenerateJwtHelper _generateJwtHelper;
        public PostTokenUseCase(IGenerateJwtHelper generateJwtHelper)
        {
            _generateJwtHelper = generateJwtHelper;
        }
        public GenerateTokenResponse Execute(TokenRequestObject tokenRequest)
        {
            //TODO call gateway to insert data into DB
            var jwtToken = _generateJwtHelper.GenerateJwtToken
                (GenerateJwtFactory.ToJwtRequest(tokenRequest, 1)); //TODO replace ID with gateway response
            if (!string.IsNullOrEmpty(jwtToken))
            {
                //TODO return token
                return new GenerateTokenResponse();
            }
            //TODO throw token could not be generated exception
            throw new NotImplementedException();
        }
    }
}
