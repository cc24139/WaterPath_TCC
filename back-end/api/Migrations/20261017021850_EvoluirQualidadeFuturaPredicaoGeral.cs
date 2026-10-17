using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_end.Migrations
{
    /// <inheritdoc />
    public partial class EvoluirQualidadeFuturaPredicaoGeral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QualidadesFuturas_CorpoHidricoId",
                schema: "waterPath",
                table: "QualidadesFuturas");

            migrationBuilder.AlterColumn<float>(
                name: "Turbidez",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<float>(
                name: "Ph",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<float>(
                name: "OxigenioDissolvido",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<float>(
                name: "Floretos",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<bool>(
                name: "EscherichiaColi",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Data",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<float>(
                name: "ColiformesTotais",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<float>(
                name: "CloroResidual",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<DateTime>(
                name: "CriadaEm",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataAlvo",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataReferencia",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HorizonteDias",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ponto",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Profundidade",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultadoJson",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualidadesFuturas_CorpoHidricoId_DataAlvo_CriadaEm",
                schema: "waterPath",
                table: "QualidadesFuturas",
                columns: new[] { "CorpoHidricoId", "DataAlvo", "CriadaEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QualidadesFuturas_CorpoHidricoId_DataAlvo_CriadaEm",
                schema: "waterPath",
                table: "QualidadesFuturas");

            migrationBuilder.DropColumn(
                name: "CriadaEm",
                schema: "waterPath",
                table: "QualidadesFuturas");

            migrationBuilder.DropColumn(
                name: "DataAlvo",
                schema: "waterPath",
                table: "QualidadesFuturas");

            migrationBuilder.DropColumn(
                name: "DataReferencia",
                schema: "waterPath",
                table: "QualidadesFuturas");

            migrationBuilder.DropColumn(
                name: "HorizonteDias",
                schema: "waterPath",
                table: "QualidadesFuturas");

            migrationBuilder.DropColumn(
                name: "Ponto",
                schema: "waterPath",
                table: "QualidadesFuturas");

            migrationBuilder.DropColumn(
                name: "Profundidade",
                schema: "waterPath",
                table: "QualidadesFuturas");

            migrationBuilder.DropColumn(
                name: "ResultadoJson",
                schema: "waterPath",
                table: "QualidadesFuturas");

            migrationBuilder.AlterColumn<float>(
                name: "Turbidez",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "Ph",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "OxigenioDissolvido",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "Floretos",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "EscherichiaColi",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Data",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "ColiformesTotais",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "CloroResidual",
                schema: "waterPath",
                table: "QualidadesFuturas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualidadesFuturas_CorpoHidricoId",
                schema: "waterPath",
                table: "QualidadesFuturas",
                column: "CorpoHidricoId");
        }
    }
}
