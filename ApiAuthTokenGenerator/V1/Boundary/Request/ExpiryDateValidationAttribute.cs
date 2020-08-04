using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.Boundary.Request
{
    public class ExpiryDateValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            //check if expiry date provided is in the future
            if (value != null)
            {
                bool isInTheFuture = (DateTime) value > DateTime.Now ? true : false;
                return isInTheFuture;
            }
            return true;
        }
    }
}
