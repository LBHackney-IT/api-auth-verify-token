using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthVerifyToken.V1.Infrastructure
{
    [Table("api_lookup")]
    public class ApiNameLookup
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }
        [Required]
        [Column("api_name")]
        public string ApiName { get; set; }

        [Column("api_gateway_id")]
        public string ApiGatewayId { get; set; }
    }
}
