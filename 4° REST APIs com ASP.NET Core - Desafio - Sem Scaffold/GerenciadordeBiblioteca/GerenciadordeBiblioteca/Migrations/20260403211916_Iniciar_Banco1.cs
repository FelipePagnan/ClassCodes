using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadordeBiblioteca.Migrations
{
    /// <inheritdoc />
    public partial class Iniciar_Banco1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotaDoLivro",
                table: "Livros",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotaDoLivro",
                table: "Livros");
        }
    }
}
