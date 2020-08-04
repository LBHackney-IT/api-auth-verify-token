using ApiAuthTokenGenerator.V1.Boundary.Request;
using ApiAuthTokenGenerator.V1.Boundary.Response;
using ApiAuthTokenGenerator.V1.Factories;
using ApiAuthTokenGenerator.V1.Helpers.Interfaces;
using ApiAuthTokenGenerator.V1.Gateways;
using ApiAuthTokenGenerator.V1.UseCase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiAuthTokenGenerator.V1.Domain;

namespace ApiAuthTokenGenerator.V1.UseCase
{
    public class PostTokenUseCase : IPostTokenUseCase
    {
        private IGenerateJwtHelper _generateJwtHelper;
        private IAuthTokenDatabaseGateway _gateway;
        public PostTokenUseCase(IGenerateJwtHelper generateJwtHelper, IAuthTokenDatabaseGateway gateway)
        {
            _generateJwtHelper = generateJwtHelper;
            _gateway = gateway;
        }
        public GenerateTokenResponse Execute(TokenRequestObject tokenRequest)
        {
            var tokenId = _gateway.GenerateToken(tokenRequest);
            if (tokenId != 0)
            {
                var jwtToken = _generateJwtHelper.GenerateJwtToken(GenerateJwtFactory.ToJwtRequest(tokenRequest, tokenId));
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    return new GenerateTokenResponse
                    {
                        Id = tokenId,
                        Token = jwtToken,
                        ExpiresAt = tokenRequest.ExpiresAt,
                        GeneratedAt = DateTime.Now
                    };
                }
                //TODO add logic to revert inserted record or update inserted record to reflect that JWT has not been generated
                throw new JwtTokenNotGeneratedException();
            }
            throw new TokenNotInsertedException();
        }
    }
}
