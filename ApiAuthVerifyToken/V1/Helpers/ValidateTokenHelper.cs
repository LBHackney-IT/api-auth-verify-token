using Amazon.Lambda.Core;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using JWT.Serializers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace ApiAuthVerifyToken.V1.Helpers
{
    public static class ValidateTokenHelper
    {



        public static List<Claim> ValidateTokenOld(string token, string secret)
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
                    ValidateAudience = false,
                    RequireExpirationTime = false,
                    RequireSignedTokens = false,
                };

                var claims = handler.ValidateToken(token, validations, out var tokenSecure);
                return claims.Claims.ToList();
            }
            catch (SecurityTokenInvalidSignatureException e)
            {
                LambdaLogger.Log($"Token beginning with {token.Substring(0, 8)} is not valid");
                LambdaLogger.Log(e.ToString());
                return null;
            }
        }


        private static string DecodeToken(string token, string secret)
        {
            try
            {
                var algorithm = new HMACSHA256Algorithm();
                var serializer = new JsonNetSerializer();
                var validator = new JwtValidator(serializer, new UtcDateTimeProvider());
                var urlEncoder = new JwtBase64UrlEncoder();
                var decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
                return decoder.Decode(token, secret, verify: true);
            }
            catch (SignatureVerificationException)
            {
                LambdaLogger.Log($"Token beginning with {token.Substring(0, 8)} is not valid");
                return null;
            }
        }


        public static List<Claim> ValidateToken(string token, string secret)
        {
            var tokenJson = DecodeToken(token, secret);
            if (tokenJson == null) return null;

            var handler = new JwtSecurityTokenHandler();
            var parsedToken = handler.ReadToken(token) as JwtSecurityToken;
            return parsedToken?.Claims.ToList() ?? new List<Claim>();
        }
    }
}
