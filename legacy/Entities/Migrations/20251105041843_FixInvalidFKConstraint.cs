using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class FixInvalidFKConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "CountryId",
                keyValue: new Guid("3c16db02-611f-40cd-ba34-43cc493ac676"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "CountryId",
                keyValue: new Guid("f6564641-ad62-41d0-9778-f289d6d434be"));

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("210780c5-945e-4a1c-9287-f5dee8e67e53"),
                column: "CountryId",
                value: new Guid("ad914740-7d42-4374-83a0-6b939e2f1f05"));

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("cd6f6dfa-71cf-4f74-ab7d-5359ac17bbea"),
                column: "CountryId",
                value: new Guid("ad914740-7d42-4374-83a0-6b939e2f1f05"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "CountryId", "CountryName" },
                values: new object[,]
                {
                    { new Guid("3c16db02-611f-40cd-ba34-43cc493ac676"), "Chile" },
                    { new Guid("f6564641-ad62-41d0-9778-f289d6d434be"), "Canada" }
                });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("210780c5-945e-4a1c-9287-f5dee8e67e53"),
                column: "CountryId",
                value: new Guid("5ca1618f-97c6-46b0-8a72-6c312a339954"));

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("cd6f6dfa-71cf-4f74-ab7d-5359ac17bbea"),
                column: "CountryId",
                value: new Guid("9a1f995b-fbab-4612-9e9c-358048815dba"));
        }
    }
}
