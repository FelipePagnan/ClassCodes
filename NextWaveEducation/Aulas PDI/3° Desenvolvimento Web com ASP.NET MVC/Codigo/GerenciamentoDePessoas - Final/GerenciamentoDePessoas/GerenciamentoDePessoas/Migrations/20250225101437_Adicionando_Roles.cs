using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GerenciamentoDePessoas.Migrations
{
    /// <inheritdoc />
    public partial class Adicionando_Roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1A7E2F64-9A1E-4A9E-9B57-2F3028E3A02D", null, "administrador", "ADMINISTRADOR" },
                    { "2B9E3F75-ABCD-4D5E-8C56-3D1238E4B03E", null, "operador", "OPERADOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1A7E2F64-9A1E-4A9E-9B57-2F3028E3A02D");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2B9E3F75-ABCD-4D5E-8C56-3D1238E4B03E");
        }
    }
}
