using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace back_end.Migrations
{
    /// <inheritdoc />
    public partial class ExpectedValueComparisons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComparacoesEsperadas",
                schema: "waterPath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ColetaId = table.Column<int>(type: "integer", nullable: false),
                    AmostraId = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Parametro = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ValorEsperado = table.Column<double>(type: "double precision", nullable: false),
                    ValorPrevisto = table.Column<double>(type: "double precision", nullable: true),
                    Unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Origem = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Sintetico = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparacoesEsperadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComparacoesEsperadas_Coletas_ColetaId",
                        column: x => x.ColetaId,
                        principalSchema: "waterPath",
                        principalTable: "Coletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComparacoesEsperadas_AmostraId_Parametro",
                schema: "waterPath",
                table: "ComparacoesEsperadas",
                columns: new[] { "AmostraId", "Parametro" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComparacoesEsperadas_ColetaId",
                schema: "waterPath",
                table: "ComparacoesEsperadas",
                column: "ColetaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComparacoesEsperadas",
                schema: "waterPath");
        }
    }
}
