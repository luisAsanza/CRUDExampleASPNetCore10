using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedDataFromContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "CountryId",
                keyValue: new Guid("ef684ba0-0ad5-41d1-8223-4122c149f9da"));

            migrationBuilder.DeleteData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("210780c5-945e-4a1c-9287-f5dee8e67e53"));

            migrationBuilder.DeleteData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("9403db58-adc5-494b-a455-b075aa7cfc46"));

            migrationBuilder.DeleteData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("cd6f6dfa-71cf-4f74-ab7d-5359ac17bbea"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "CountryId",
                keyValue: new Guid("08852737-942a-48e2-8622-6654af313a09"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "CountryId",
                keyValue: new Guid("ad914740-7d42-4374-83a0-6b939e2f1f05"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "CountryId", "CountryName" },
                values: new object[,]
                {
                    { new Guid("08852737-942a-48e2-8622-6654af313a09"), "China" },
                    { new Guid("ad914740-7d42-4374-83a0-6b939e2f1f05"), "Brazil" },
                    { new Guid("ef684ba0-0ad5-41d1-8223-4122c149f9da"), "Argentina" }
                });

            migrationBuilder.InsertData(
                table: "Persons",
                columns: new[] { "PersonId", "Address", "CountryId", "DateOfBirth", "Email", "Gender", "PersonName", "ReceiveNewsLetters" },
                values: new object[,]
                {
                    { new Guid("210780c5-945e-4a1c-9287-f5dee8e67e53"), "688 Main St, Cityville", new Guid("ad914740-7d42-4374-83a0-6b939e2f1f05"), new DateOnly(2005, 8, 29), "carlos.rivera@example.com", "Female", "Carlos Rivera", true },
                    { new Guid("9403db58-adc5-494b-a455-b075aa7cfc46"), "913 Main St, Cityville", new Guid("08852737-942a-48e2-8622-6654af313a09"), new DateOnly(1962, 11, 25), "alice.johnson@example.com", "Other", "Alice Johnson", true },
                    { new Guid("cd6f6dfa-71cf-4f74-ab7d-5359ac17bbea"), "332 Main St, Cityville", new Guid("ad914740-7d42-4374-83a0-6b939e2f1f05"), new DateOnly(2004, 3, 5), "bob.smith@example.com", "Male", "Bob Smith", true }
                });
        }
    }
}
