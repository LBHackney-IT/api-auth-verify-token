using System;
using System.ComponentModel.DataAnnotations;

namespace ApiAuthTokenManagement.V1.Boundary.Request
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
