using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace back_end.Migrations
{
    /// <inheritdoc />
    public partial class PersistirPredicoesIA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PredicoesIA",
                schema: "waterPath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ColetaId = table.Column<int>(type: "integer", nullable: false),
                    CriadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EntradaJson = table.Column<string>(type: "jsonb", nullable: true),
                    ResultadoJson = table.Column<string>(type: "jsonb", nullable: true),
                    NomeArquivo = table.Column<string>(type: "text", nullable: false),
                    ContentTypeOriginal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImagemOriginal = table.Column<byte[]>(type: "bytea", nullable: false),
                    ContentTypeResultado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImagemResultado = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredicoesIA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PredicoesIA_Coletas_ColetaId",
                        column: x => x.ColetaId,
                        principalSchema: "waterPath",
                        principalTable: "Coletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PredicoesIA_ColetaId_CriadaEm",
                schema: "waterPath",
                table: "PredicoesIA",
                columns: new[] { "ColetaId", "CriadaEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PredicoesIA",
                schema: "waterPath");
        }
    }
}
