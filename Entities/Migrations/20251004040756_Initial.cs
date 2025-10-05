using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CountryName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.CountryId);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReceiveNewsLetters = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.PersonId);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "CountryId", "CountryName" },
                values: new object[,]
                {
                    { new Guid("08852737-942a-48e2-8622-6654af313a09"), "China" },
                    { new Guid("3c16db02-611f-40cd-ba34-43cc493ac676"), "Chile" },
                    { new Guid("ad914740-7d42-4374-83a0-6b939e2f1f05"), "Brazil" },
                    { new Guid("ef684ba0-0ad5-41d1-8223-4122c149f9da"), "Argentina" },
                    { new Guid("f6564641-ad62-41d0-9778-f289d6d434be"), "Canada" }
                });

            migrationBuilder.InsertData(
                table: "Persons",
                columns: new[] { "PersonId", "Address", "CountryId", "DateOfBirth", "Email", "Gender", "PersonName", "ReceiveNewsLetters" },
                values: new object[,]
                {
                    { new Guid("210780c5-945e-4a1c-9287-f5dee8e67e53"), "688 Main St, Cityville", new Guid("5ca1618f-97c6-46b0-8a72-6c312a339954"), new DateOnly(2005, 8, 29), "carlos.rivera@example.com", "Female", "Carlos Rivera", true },
                    { new Guid("9403db58-adc5-494b-a455-b075aa7cfc46"), "913 Main St, Cityville", new Guid("08852737-942a-48e2-8622-6654af313a09"), new DateOnly(1962, 11, 25), "alice.johnson@example.com", "Other", "Alice Johnson", true },
                    { new Guid("cd6f6dfa-71cf-4f74-ab7d-5359ac17bbea"), "332 Main St, Cityville", new Guid("9a1f995b-fbab-4612-9e9c-358048815dba"), new DateOnly(2004, 3, 5), "bob.smith@example.com", "Male", "Bob Smith", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Persons");
        }
    }
}
