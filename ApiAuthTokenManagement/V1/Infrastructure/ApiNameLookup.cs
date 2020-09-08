using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAuthTokenManagement.V1.Infrastructure
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
    }
}
