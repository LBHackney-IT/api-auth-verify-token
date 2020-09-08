using System;
using ApiAuthTokenManagement.V1.Boundary.Request;
using ApiAuthTokenManagement.V1.Boundary.Response;
using ApiAuthTokenManagement.V1.Domain.Exceptions;
using ApiAuthTokenManagement.V1.Factories;
using ApiAuthTokenManagement.V1.Gateways;
using ApiAuthTokenManagement.V1.UseCase.Interfaces;

namespace ApiAuthTokenManagement.V1.UseCase
{
    public class PostTokenUseCase : IPostTokenUseCase
    {
        private IGenerateJwt _generateJwt;
        private IAuthTokenDatabaseGateway _gateway;
        public PostTokenUseCase(IGenerateJwt generateJwt, IAuthTokenDatabaseGateway gateway)
        {
            _generateJwt = generateJwt;
            _gateway = gateway;
        }
        public GenerateTokenResponse Execute(TokenRequestObject tokenRequest)
        {
            var tokenId = _gateway.GenerateToken(tokenRequest);
            if (tokenId != 0)
            {
                var jwtToken = _generateJwt.Execute(GenerateJwtFactory.ToJwtRequest(tokenRequest, tokenId));
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
