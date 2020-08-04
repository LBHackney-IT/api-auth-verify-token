using Microsoft.EntityFrameworkCore;

namespace ApiAuthTokenGenerator.V1.Infrastructure
{

    public class TokenDatabaseContext : DbContext
    {
        //Guidance on the context class can be found here https://github.com/LBHackney-IT/lbh-base-api/wiki/DatabaseContext
        public TokenDatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AuthTokens> Tokens { get; set; }
        public DbSet<ApiNameLookup> ApiNameLookups { get; set; }
        public DbSet<ApiEndpointNameLookup> ApiEndpointNameLookups { get; set; }
        public DbSet<ConsumerTypeLookup> ConsumerTypeLookups { get; set; }
    }
}
