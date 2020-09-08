using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ApiAuthTokenManagement.V1.Boundary.Request
{
    public class HttpMethodTypeValidationAttribute : ValidationAttribute
    {
        private readonly List<string> _httpMethodTypes = new List<string> { "GET", "POST", "DELETE", "PUT", "PATCH" };
        public override bool IsValid(object value)
        {
            if (value != null && _httpMethodTypes.Contains(value.ToString().ToUpper(CultureInfo.InvariantCulture)))
            {
                return true;
            }
            return false;
        }
    }
}
