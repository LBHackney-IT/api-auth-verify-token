using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAuthTokenManagement.V1.Infrastructure
{
    [Table("api_endpoint_lookup")]
    public class ApiEndpointNameLookup
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }
        [Required]
        [Column("endpoint_name")]
        public string ApiEndpointName { get; set; }
        [Required]
        [Column("api_lookup_id")]
        [ForeignKey("api_lookup")]
        public int ApiLookupId { get; set; }
    }
}
