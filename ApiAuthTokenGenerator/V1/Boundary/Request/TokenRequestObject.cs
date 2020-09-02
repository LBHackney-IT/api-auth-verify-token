using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.Boundary.Request
{
    public class TokenRequestObject
    {
        /// <example>
        /// john.smith@test.com
        /// </example>
        [Required]
        public string RequestedBy { get; set; }
        /// <example>
        /// anna.smith@test.com
        /// </example>
        [Required]
        public string AuthorizedBy { get; set; }
        /// <example>
        /// service
        /// </example>
        [Required]
        public int ConsumerType { get; set; }
        /// <example>
        /// MaT
        /// </example>
        [Required]
        public string Consumer { get; set; }
        /// <example>
        /// tenancy-information-api
        /// </example>
        [Required]
        public int ApiName { get; set; }
        /// <example>
        /// /tenancies
        /// </example>
        [Required]
        public int ApiEndpoint { get; set; }
        /// <example>
        /// GET
        /// </example>
        [Required]
        [MaxLength(6)]
        [HttpMethodTypeValidation(ErrorMessage = "Please provide a valid HTTP method type")]
        public string HttpMethodType { get; set; }
        /// <example>
        /// staging
        /// </example>
        [Required]
        public string Environment { get; set; }
        /// <example>
        /// 2020-05-15
        /// </example>
        [Required]
        public DateTime DateRequested { get; set; }
        /// <example>
        /// 2020-05-15
        /// </example>
        [ExpiryDateValidationAttribute(ErrorMessage = "Token expiry date should be a future date")]
        public DateTime? ExpiresAt { get; set; }
    }
}
