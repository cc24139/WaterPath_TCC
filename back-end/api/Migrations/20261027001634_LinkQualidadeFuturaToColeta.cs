using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_end.Migrations
{
    /// <inheritdoc />
    public partial class LinkQualidadeFuturaToColeta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColetaId",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualidadesFuturas_ColetaId",
                schema: "waterPath",
                table: "QualidadesFuturas",
                column: "ColetaId");

            migrationBuilder.AddForeignKey(
                name: "FK_QualidadesFuturas_Coletas_ColetaId",
                schema: "waterPath",
                table: "QualidadesFuturas",
                column: "ColetaId",
                principalSchema: "waterPath",
                principalTable: "Coletas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QualidadesFuturas_Coletas_ColetaId",
                schema: "waterPath",
                table: "QualidadesFuturas");

            migrationBuilder.DropIndex(
                name: "IX_QualidadesFuturas_ColetaId",
                schema: "waterPath",
                table: "QualidadesFuturas");

            migrationBuilder.DropColumn(
                name: "ColetaId",
                schema: "waterPath",
                table: "QualidadesFuturas");
        }
    }
}
