using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.Helpers
{
    public static class ValidateTokenHelper
    {
        public static List<Claim> ValidateToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var validations = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes
                    (Environment.GetEnvironmentVariable("jwtSecret"))),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                var claims = handler.ValidateToken(token, validations, out var tokenSecure);
                return claims.Claims.ToList();
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return null; //token invalid, return null
            }
        }
    }
}
