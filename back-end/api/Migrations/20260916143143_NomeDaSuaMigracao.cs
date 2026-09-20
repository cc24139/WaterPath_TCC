using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace back_end.Migrations
{
    /// <inheritdoc />
    public partial class NomeDaSuaMigracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Codigos",
                schema: "waterPath");

            migrationBuilder.DropColumn(
                name: "CloroResidual",
                schema: "waterPath",
                table: "Coletas");

            migrationBuilder.DropColumn(
                name: "ColiformesTotais",
                schema: "waterPath",
                table: "Coletas");

            migrationBuilder.DropColumn(
                name: "EscherichiaColi",
                schema: "waterPath",
                table: "Coletas");

            migrationBuilder.DropColumn(
                name: "Floretos",
                schema: "waterPath",
                table: "Coletas");

            migrationBuilder.DropColumn(
                name: "Nome",
                schema: "waterPath",
                table: "Coletas");

            migrationBuilder.DropColumn(
                name: "OxigenioDissolvido",
                schema: "waterPath",
                table: "Coletas");

            migrationBuilder.DropColumn(
                name: "Ph",
                schema: "waterPath",
                table: "Coletas");

            migrationBuilder.DropColumn(
                name: "Turbidez",
                schema: "waterPath",
                table: "Coletas");

            migrationBuilder.RenameColumn(
                name: "Data",
                schema: "waterPath",
                table: "Coletas",
                newName: "DataHora");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                schema: "waterPath",
                table: "Coletas",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                schema: "waterPath",
                table: "Coletas",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ProfundidadeMetros",
                schema: "waterPath",
                table: "Coletas",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Medicoes",
                schema: "waterPath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ColetaId = table.Column<int>(type: "integer", nullable: false),
                    codigoMedicao = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<double>(type: "double precision", nullable: true),
                    unidade = table.Column<string>(type: "text", nullable: false),
                    censurado = table.Column<bool>(type: "boolean", nullable: false),
                    limite = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medicoes_Coletas_ColetaId",
                        column: x => x.ColetaId,
                        principalSchema: "waterPath",
                        principalTable: "Coletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medicoes_ColetaId_codigoMedicao",
                schema: "waterPath",
                table: "Medicoes",
                columns: new[] { "ColetaId", "codigoMedicao" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medicoes",
                schema: "waterPath");

            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "waterPath",
                table: "Coletas");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "waterPath",
                table: "Coletas");

            migrationBuilder.DropColumn(
                name: "ProfundidadeMetros",
                schema: "waterPath",
                table: "Coletas");

            migrationBuilder.RenameColumn(
                name: "DataHora",
                schema: "waterPath",
                table: "Coletas",
                newName: "Data");

            migrationBuilder.AddColumn<float>(
                name: "CloroResidual",
                schema: "waterPath",
                table: "Coletas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "ColiformesTotais",
                schema: "waterPath",
                table: "Coletas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<bool>(
                name: "EscherichiaColi",
                schema: "waterPath",
                table: "Coletas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "Floretos",
                schema: "waterPath",
                table: "Coletas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                schema: "waterPath",
                table: "Coletas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "OxigenioDissolvido",
                schema: "waterPath",
                table: "Coletas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Ph",
                schema: "waterPath",
                table: "Coletas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Turbidez",
                schema: "waterPath",
                table: "Coletas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateTable(
                name: "Codigos",
                schema: "waterPath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataGeracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Usado = table.Column<bool>(type: "boolean", nullable: false),
                    emailUsuario = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Codigos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Codigos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "waterPath",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Codigos_UsuarioId",
                schema: "waterPath",
                table: "Codigos",
                column: "UsuarioId");
        }
    }
}
