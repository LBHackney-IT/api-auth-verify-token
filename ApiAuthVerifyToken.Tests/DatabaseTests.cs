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
    }
}
