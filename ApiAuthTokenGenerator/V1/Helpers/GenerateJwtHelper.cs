using ApiAuthTokenGenerator.V1.Boundary;
using ApiAuthTokenGenerator.V1.Boundary.Request;
using ApiAuthTokenGenerator.V1.Helpers.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.Helpers
{
    public class GenerateJwtHelper : IGenerateJwtHelper
    {
        public string GenerateJwtToken(GenerateJwtRequest jwtRequestObject)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("jwtSecret"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id", jwtRequestObject.Id.ToString(CultureInfo.InvariantCulture)),
                    new Claim("consumerName", jwtRequestObject.ConsumerName),
                    new Claim("consumerType", jwtRequestObject.ConsumerType),
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            if (jwtRequestObject.ExpiresAt != DateTime.MinValue)
            {
                //expiration date has been provided
                tokenDescriptor.Expires = jwtRequestObject.ExpiresAt;
            }
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
