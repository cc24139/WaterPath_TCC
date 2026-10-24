using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_end.Migrations;

public partial class AnalysisIdempotency : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(name: "CadastroRequestKey", schema: "waterPath", table: "Coletas",
            type: "uuid", nullable: true);
        migrationBuilder.AddColumn<string>(name: "CadastroRequestHash", schema: "waterPath", table: "Coletas",
            type: "character varying(64)", maxLength: 64, nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "AnaliseRequestKey", schema: "waterPath", table: "PredicoesIA",
            type: "uuid", nullable: true);
        migrationBuilder.AddColumn<string>(name: "AnaliseRequestHash", schema: "waterPath", table: "PredicoesIA",
            type: "character varying(64)", maxLength: 64, nullable: true);
        migrationBuilder.CreateIndex(name: "IX_Coletas_CadastroRequestKey", schema: "waterPath", table: "Coletas",
            column: "CadastroRequestKey", unique: true);
        migrationBuilder.CreateIndex(name: "IX_PredicoesIA_AnaliseRequestKey", schema: "waterPath", table: "PredicoesIA",
            column: "AnaliseRequestKey", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_PredicoesIA_AnaliseRequestKey", schema: "waterPath", table: "PredicoesIA");
        migrationBuilder.DropIndex(name: "IX_Coletas_CadastroRequestKey", schema: "waterPath", table: "Coletas");
        migrationBuilder.DropColumn(name: "AnaliseRequestHash", schema: "waterPath", table: "PredicoesIA");
        migrationBuilder.DropColumn(name: "AnaliseRequestKey", schema: "waterPath", table: "PredicoesIA");
        migrationBuilder.DropColumn(name: "CadastroRequestHash", schema: "waterPath", table: "Coletas");
        migrationBuilder.DropColumn(name: "CadastroRequestKey", schema: "waterPath", table: "Coletas");
    }
}
