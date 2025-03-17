using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthVerifyTokenNew.V1.Infrastructure
{
    [Table("consumer_type_lookup")]
    public class ConsumerTypeLookup
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }
        [Required]
        [Column("consumer_name")]
        public string TypeName { get; set; }
    }
}
