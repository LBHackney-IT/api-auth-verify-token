using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

namespace ApiAuthVerifyToken.V1.Helpers
{
    public static class DetermineTokenSourceHelper
    {
        public static bool DetermineTokenSource(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            //if consumerType is present, this is service auth flow
            return jsonToken.Claims.Any(claim => claim.Type == "consumerType");
        }
    }
}
