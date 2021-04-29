using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ApiAuthVerifyToken.V1.Helpers
{
    public static class ValidateTokenHelper
    {
        public static List<Claim> ValidateToken(string token,string secret)
        {
            try
            {
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                var handler = new JwtSecurityTokenHandler();
                var validations = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                var claims = handler.ValidateToken(token, validations, out var tokenSecure);
                return claims.Claims.ToList();
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                LambdaLogger.Log($"Token beginning with {token.Substring(0, 8)} is not valid");
                return null; //token invalid, return null
            }
        }
    }
}
