using System;
using ApiAuthVerifyToken.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ApiAuthVerifyToken.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        protected TokenDatabaseContext DatabaseContext { get; private set; }

        [SetUp]
        public void RunBeforeAnyTests()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var builder = new DbContextOptionsBuilder();
            builder.UseNpgsql(ConnectionString.TestDatabase());
            DatabaseContext = new TokenDatabaseContext(builder.Options);
            DatabaseContext.Database.EnsureCreated();
            DatabaseContext.Database.OpenConnection();
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            DatabaseContext.Database.CloseConnection();
        }

        public void StoreTokenDataInDatabase(ApiAuthVerifyToken.V1.Domain.AuthTokenServiceFlow tokenData)
        {
            var connection = DatabaseContext.Database.GetDbConnection() as Npgsql.NpgsqlConnection;
            int apiLookupId = InsertAndReturnId(connection,
            "INSERT INTO api_lookup (api_name) VALUES (@apiName) RETURNING id",
            (cmd) => cmd.Parameters.AddWithValue("@apiName", tokenData.ApiName));

            int apiEndpointLookupId = InsertAndReturnId(connection,
            "INSERT INTO api_endpoint_lookup (endpoint_name, api_lookup_id) VALUES (@endpointName, @apiLookupId) RETURNING id",
            (cmd) =>
            {
                cmd.Parameters.AddWithValue("@endpointName", tokenData.ApiEndpointName);
                cmd.Parameters.AddWithValue("@apiLookupId", apiLookupId);
            });

            int consumerTypeLookupId = InsertAndReturnId(connection,
            "INSERT INTO consumer_type_lookup (consumer_name) VALUES (@consumerName) ON CONFLICT DO NOTHING RETURNING id",
            (cmd) => cmd.Parameters.AddWithValue("@consumerName", tokenData.ConsumerType));

            tokenData.Id = InsertAndReturnId(connection,
            @"INSERT INTO tokens (api_lookup_id, api_endpoint_lookup_id, http_method_type, environment, consumer_name, consumer_type_lookup, requested_by, authorized_by, date_created, expiration_date, enabled)
              VALUES (@apiLookupId, @apiEndpointLookupId, @httpMethodType, @environment, @consumerName, @consumerTypeLookupId, @requestedBy, @authorizedBy, @dateCreated, @expirationDate, @enabled)
              RETURNING id",
            (cmd) =>
            {
                cmd.Parameters.AddWithValue("@apiLookupId", apiLookupId);
                cmd.Parameters.AddWithValue("@apiEndpointLookupId", apiEndpointLookupId);
                cmd.Parameters.AddWithValue("@httpMethodType", tokenData.HttpMethodType);
                cmd.Parameters.AddWithValue("@environment", tokenData.Environment);
                cmd.Parameters.AddWithValue("@consumerName", tokenData.ConsumerName);
                cmd.Parameters.AddWithValue("@consumerTypeLookupId", consumerTypeLookupId);
                cmd.Parameters.AddWithValue("@requestedBy", "E2ETest");
                cmd.Parameters.AddWithValue("@authorizedBy", "E2ETest");
                cmd.Parameters.AddWithValue("@dateCreated", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@expirationDate", tokenData.ExpirationDate.HasValue ? tokenData.ExpirationDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@enabled", tokenData.Enabled);
            });
        }

        private static int InsertAndReturnId(Npgsql.NpgsqlConnection connection, string sql, Action<Npgsql.NpgsqlCommand> addParams)
        {
#pragma warning disable CA2100 // No sql injection risk - this is for a test
            using (var cmd = new Npgsql.NpgsqlCommand(sql, connection))
#pragma warning restore CA2100
            {
                addParams(cmd);
                var result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    throw new InvalidOperationException("Insert did not return an id.");
                return (int) result;
            }
        }
    }
}
