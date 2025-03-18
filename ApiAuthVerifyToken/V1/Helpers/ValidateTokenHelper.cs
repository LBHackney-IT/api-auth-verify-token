using Amazon.Lambda.Core;
using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace ApiAuthVerifyToken.V1.Helpers
{
    public static class ValidateTokenHelper
    {
        private static readonly JwtDecoder _decoder;

        static ValidateTokenHelper()
        {
            var algorithm = new HMACSHA256Algorithm();
            var serializer = new JsonNetSerializer();
            var validator = new JwtValidator(serializer, new UtcDateTimeProvider());
            var urlEncoder = new JwtBase64UrlEncoder();

            _decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
        }

        public static List<Claim> ValidateToken(string token, string secret)
        {
            try
            {
                var tokenJson = _decoder.Decode(token, secret, verify: true);
                if (tokenJson == null) return null;

                return ExtractClaims(token);
            }
            catch (SignatureVerificationException)
            {
                LambdaLogger.Log($"Token beginning with {token.Substring(0, 8)} is not valid");
                return null;
            }
        }

        private static List<Claim> ExtractClaims(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var parsedToken = handler.ReadToken(token) as JwtSecurityToken;
            return parsedToken?.Claims.ToList() ?? new List<Claim>();
        }
    }
}
