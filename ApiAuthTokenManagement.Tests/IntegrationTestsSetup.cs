using System.Net.Http;
using ApiAuthTokenManagement.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NUnit.Framework;

namespace ApiAuthTokenManagement.Tests
{
    public class IntegrationTestsSetup<TStartup> where TStartup : class
    {
        protected HttpClient Client { get; private set; }
        protected TokenDatabaseContext DatabaseContext { get; private set; }
        private MockWebApplicationFactory<TStartup> _factory;
        private NpgsqlConnection _connection;
        [SetUp]
        public void BaseSetup()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseNpgsql(ConnectionString.TestDatabase());
            DatabaseContext = new TokenDatabaseContext(builder.Options);
            DatabaseContext.Database.EnsureCreated();

            _connection = new NpgsqlConnection(ConnectionString.TestDatabase());
            _connection.Open();

            _factory = new MockWebApplicationFactory<TStartup>(_connection);
            Client = _factory.CreateClient();
        }

        [TearDown]
        public void BaseTearDown()
        {
            Client.Dispose();
            _factory.Dispose();
            _connection.Close();
        }
    }
}
