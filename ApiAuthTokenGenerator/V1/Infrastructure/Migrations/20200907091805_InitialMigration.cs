using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ApiAuthTokenGenerator.V1.Infrastructure.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "api_endpoint_lookup",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    endpoint_name = table.Column<string>(nullable: false),
                    api_lookup_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_endpoint_lookup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "api_lookup",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    api_name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_lookup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "consumer_type_lookup",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    consumer_name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consumer_type_lookup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tokens",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    api_lookup_id = table.Column<int>(nullable: false),
                    api_endpoint_lookup_id = table.Column<int>(nullable: false),
                    http_method_type = table.Column<string>(maxLength: 6, nullable: false),
                    environment = table.Column<string>(nullable: false),
                    consumer_name = table.Column<string>(nullable: false),
                    consumer_type_lookup = table.Column<int>(nullable: false),
                    requested_by = table.Column<string>(nullable: false),
                    authorized_by = table.Column<string>(nullable: false),
                    date_created = table.Column<DateTime>(nullable: false),
                    expiration_date = table.Column<DateTime>(nullable: true),
                    enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tokens", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_endpoint_lookup");

            migrationBuilder.DropTable(
                name: "api_lookup");

            migrationBuilder.DropTable(
                name: "consumer_type_lookup");

            migrationBuilder.DropTable(
                name: "tokens");
        }
    }
}
