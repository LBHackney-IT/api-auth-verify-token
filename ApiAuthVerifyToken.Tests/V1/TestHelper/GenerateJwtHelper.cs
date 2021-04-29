using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bogus;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace ApiAuthVerifyToken.Tests.V1.TestHelper
{
    public static class GenerateJwtHelper
    {
        public static string GenerateJwtToken(DateTime? expiresAt = null)
        {
            var faker = new Faker();
            var requestDetails = new
            {
                ConsumerName = faker.Name.FullName(),
                ConsumerType = faker.Random.Int(1, 2),
                ExpiresAt = expiresAt,
                Id = faker.Random.Int(1, 100)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("jwtSecret"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id", requestDetails.Id.ToString(CultureInfo.InvariantCulture)),
                    new Claim("consumerName", requestDetails.ConsumerName),
                    new Claim("consumerType", requestDetails.ConsumerType.ToString(CultureInfo.InvariantCulture)),
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Expires = requestDetails.ExpiresAt ?? DateTime.Now.AddYears(10)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public static string GenerateJwtTokenUserFlow(List<string> groups)
        {
            var faker = new Faker();
            var requestDetails = new
            {
                email = faker.Person.Email,
                name = faker.Name.FullName(),
                groups = JsonConvert.SerializeObject(groups)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("hackneyUserAuthTokenJwtSecret"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("email", requestDetails.email),
                    new Claim("name", requestDetails.name),
                    new Claim("groups", requestDetails.groups),
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Expires = DateTime.Now.AddYears(10)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
