using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAuthTokenGenerator.V1.Infrastructure
{
    // There's an example of this in the wiki https://github.com/LBHackney-IT/lbh-base-api/wiki/DatabaseContext
    [Table("tokens")]
    public class AuthTokens
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }
        [Column("api_lookup_id")]
        [ForeignKey("api_lookup")]
        public int ApiLookupId { get; set; }
        [Column("api_endpoint_lookup_id")]
        [ForeignKey("api_endpoint_lookup")]
        public int ApiEndpointNameLookupId { get; set; }
        [MaxLength(6)]
        [Column("http_method_type")]
        public string HttpMethodType { get; set; }
        [Column("environment")]
        public string Environment { get; set; }
        [Column("consumer_name")]
        public string ConsumerName { get; set; }
        [Column("consumer_type_lookup")]
        [ForeignKey("consumer_type_lookup")]
        public int ConsumerTypeLookupId { get; set; }
        [Column("requested_by")]
        public string RequestedBy { get; set; }
        [Column("authorized_by")]
        public string AuthorizedBy { get; set; }
        [Column("date_created")]
        public DateTime DateCreated { get; set; }
        [Column("expiration_date")]
        public DateTime? ExpirationDate { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }
    }
}
