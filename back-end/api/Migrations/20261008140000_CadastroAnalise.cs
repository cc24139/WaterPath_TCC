using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_end.Migrations;

public partial class CadastroAnalise : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Identidade do cadastro; não atribui responsáveis a registros antigos.
        migrationBuilder.AddColumn<int>(name: "ResponsavelId", schema: "waterPath", table: "Coletas", type: "integer", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ResponsavelNome", schema: "waterPath", table: "Coletas", type: "text", nullable: true);
        migrationBuilder.AlterColumn<double>(name: "Concentracao", schema: "waterPath", table: "MetaisPesados",
            type: "double precision", nullable: false, oldClrType: typeof(float), oldType: "real");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ResponsavelId", schema: "waterPath", table: "Coletas");
        migrationBuilder.DropColumn(name: "ResponsavelNome", schema: "waterPath", table: "Coletas");
        migrationBuilder.AlterColumn<float>(name: "Concentracao", schema: "waterPath", table: "MetaisPesados",
            type: "real", nullable: false, oldClrType: typeof(double), oldType: "double precision");
    }
}
