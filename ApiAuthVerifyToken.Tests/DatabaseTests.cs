using System;
using ApiAuthVerifyToken.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
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
            using (var apiLookupCmd = new Npgsql.NpgsqlCommand(@"INSERT INTO api_lookup (api_name) VALUES (@apiName) RETURNING id", connection))
            {
                apiLookupCmd.Parameters.AddWithValue("@apiName", ValidateInput(tokenData.ApiName));
                var apiLookupId = (int) apiLookupCmd.ExecuteScalar();
                using (var apiEndpointLookupCmd = new Npgsql.NpgsqlCommand(@"INSERT INTO api_endpoint_lookup (endpoint_name, api_lookup_id) VALUES (@endpointName, @apiLookupId) RETURNING id", connection))
                {
                    apiEndpointLookupCmd.Parameters.AddWithValue("@endpointName", ValidateInput(tokenData.ApiEndpointName));
                    apiEndpointLookupCmd.Parameters.AddWithValue("@apiLookupId", apiLookupId);
                    var apiEndpointLookupId = (int) apiEndpointLookupCmd.ExecuteScalar();
                    int consumerTypeLookupId;
                    using (var consumerTypeLookupCmd = new Npgsql.NpgsqlCommand(@"INSERT INTO consumer_type_lookup (consumer_name) VALUES (@consumerName) ON CONFLICT DO NOTHING RETURNING id", connection))
                    {
                        consumerTypeLookupCmd.Parameters.AddWithValue("@consumerName", ValidateInput(tokenData.ConsumerType));
                        var result = consumerTypeLookupCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            using (var getIdCmd = new Npgsql.NpgsqlCommand("SELECT id FROM consumer_type_lookup WHERE consumer_name = @consumerName", connection))
                            {
                                getIdCmd.Parameters.AddWithValue("@consumerName", ValidateInput(tokenData.ConsumerType));
                                result = getIdCmd.ExecuteScalar();
                                if (result == null || result == DBNull.Value)
                                    throw new InvalidOperationException($"Could not find consumer type '{tokenData.ConsumerType}' after insert attempt.");
                                consumerTypeLookupId = (int) result;
                            }
                        }
                        else
                        {
                            consumerTypeLookupId = (int) result;
                        }
                    }
                    using (var tokensCmd = new Npgsql.NpgsqlCommand(@"INSERT INTO tokens (api_lookup_id, api_endpoint_lookup_id, http_method_type, environment, consumer_name, consumer_type_lookup, requested_by, authorized_by, date_created, expiration_date, enabled) VALUES (@apiLookupId, @apiEndpointLookupId, @httpMethodType, @environment, @consumerName, @consumerTypeLookupId, @requestedBy, @authorizedBy, @dateCreated, @expirationDate, @enabled) RETURNING id", connection))
                    {
                        tokensCmd.Parameters.AddWithValue("@apiLookupId", apiLookupId);
                        tokensCmd.Parameters.AddWithValue("@apiEndpointLookupId", apiEndpointLookupId);
                        tokensCmd.Parameters.AddWithValue("@httpMethodType", ValidateInput(tokenData.HttpMethodType, 6));
                        tokensCmd.Parameters.AddWithValue("@environment", ValidateInput(tokenData.Environment, 255));
                        tokensCmd.Parameters.AddWithValue("@consumerName", ValidateInput(tokenData.ConsumerName, 255));
                        tokensCmd.Parameters.AddWithValue("@consumerTypeLookupId", consumerTypeLookupId);
                        tokensCmd.Parameters.AddWithValue("@requestedBy", ValidateInput("E2ETest", 255));
                        tokensCmd.Parameters.AddWithValue("@authorizedBy", ValidateInput("E2ETest", 255));
                        tokensCmd.Parameters.AddWithValue("@dateCreated", DateTime.UtcNow);
                        tokensCmd.Parameters.AddWithValue("@expirationDate", tokenData.ExpirationDate.HasValue ? (object) tokenData.ExpirationDate.Value : DBNull.Value);
                        tokensCmd.Parameters.AddWithValue("@enabled", tokenData.Enabled);
                        tokenData.Id = (int) tokensCmd.ExecuteScalar();
                    }
                }
            }
        }

        private static string ValidateInput(string input, int maxLength = 50)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input = input.Trim();
            return input.Length > maxLength ? input.Substring(0, maxLength) : input;
        }
    }
}
