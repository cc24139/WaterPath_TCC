using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_end.Migrations
{
    [Migration("20261022000000_AddQualityMeasurements")]
    public partial class AddQualityMeasurements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ColetaId",
                schema: "waterPath",
                table: "Medicoes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "QualidadeId",
                schema: "waterPath",
                table: "Medicoes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QualidadeFuturaId",
                schema: "waterPath",
                table: "Medicoes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medicoes_QualidadeId_codigoMedicao",
                schema: "waterPath",
                table: "Medicoes",
                columns: new[] { "QualidadeId", "codigoMedicao" });

            migrationBuilder.CreateIndex(
                name: "IX_Medicoes_QualidadeFuturaId_codigoMedicao",
                schema: "waterPath",
                table: "Medicoes",
                columns: new[] { "QualidadeFuturaId", "codigoMedicao" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Medicoes_ExactlyOneOwner",
                schema: "waterPath",
                table: "Medicoes",
                sql: "(\"ColetaId\" IS NOT NULL AND \"QualidadeId\" IS NULL AND \"QualidadeFuturaId\" IS NULL) OR " +
                    "(\"ColetaId\" IS NULL AND \"QualidadeId\" IS NOT NULL AND \"QualidadeFuturaId\" IS NULL) OR " +
                    "(\"ColetaId\" IS NULL AND \"QualidadeId\" IS NULL AND \"QualidadeFuturaId\" IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Medicoes_Qualidades_QualidadeId",
                schema: "waterPath",
                table: "Medicoes",
                column: "QualidadeId",
                principalSchema: "waterPath",
                principalTable: "Qualidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Medicoes_QualidadesFuturas_QualidadeFuturaId",
                schema: "waterPath",
                table: "Medicoes",
                column: "QualidadeFuturaId",
                principalSchema: "waterPath",
                principalTable: "QualidadesFuturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DO $$ BEGIN IF EXISTS (SELECT 1 FROM \"waterPath\".\"Medicoes\" WHERE \"ColetaId\" IS NULL) THEN RAISE EXCEPTION 'Cannot downgrade AddQualityMeasurements: quality-owned measurements have no ColetaId'; END IF; END $$;");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Medicoes_ExactlyOneOwner",
                schema: "waterPath",
                table: "Medicoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Medicoes_Qualidades_QualidadeId",
                schema: "waterPath",
                table: "Medicoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Medicoes_QualidadesFuturas_QualidadeFuturaId",
                schema: "waterPath",
                table: "Medicoes");

            migrationBuilder.DropIndex(
                name: "IX_Medicoes_QualidadeId_codigoMedicao",
                schema: "waterPath",
                table: "Medicoes");

            migrationBuilder.DropIndex(
                name: "IX_Medicoes_QualidadeFuturaId_codigoMedicao",
                schema: "waterPath",
                table: "Medicoes");

            migrationBuilder.DropColumn(
                name: "QualidadeId",
                schema: "waterPath",
                table: "Medicoes");

            migrationBuilder.DropColumn(
                name: "QualidadeFuturaId",
                schema: "waterPath",
                table: "Medicoes");

            migrationBuilder.AlterColumn<int>(
                name: "ColetaId",
                schema: "waterPath",
                table: "Medicoes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
