using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.Infrastructure
{
    [Table("api_endpoint_lookup")]
    public class ApiEndpointNameLookup
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }
        [Column("api_endpoint_name")]
        public string ApiEndpointName { get; set; }
        [Column("api_name_lookup")]
        [ForeignKey("api_name_lookup")]
        public string ApiNameLookupId { get; set; }
    }
}
