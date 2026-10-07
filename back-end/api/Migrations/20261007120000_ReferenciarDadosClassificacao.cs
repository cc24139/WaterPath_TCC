using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_end.Migrations;

public partial class ReferenciarDadosClassificacao : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // A coleta atual pode ter sido editada desde a análise. Não inventa o instante/entidade legados.
        migrationBuilder.AddColumn<int>(name: "CorpoHidricoId", schema: "waterPath", table: "PredicoesIA",
            type: "integer", nullable: true);
        migrationBuilder.AddColumn<DateTime>(name: "DataColeta", schema: "waterPath", table: "PredicoesIA",
            type: "timestamp with time zone", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "CorpoHidricoId", schema: "waterPath", table: "PredicoesIA");
        migrationBuilder.DropColumn(name: "DataColeta", schema: "waterPath", table: "PredicoesIA");
    }
}
