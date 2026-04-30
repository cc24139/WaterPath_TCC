using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace back_end.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatePostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "waterPath");

            migrationBuilder.CreateTable(
                name: "CorposHidricos",
                schema: "waterPath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Localizacao = table.Column<string>(type: "text", nullable: false),
                    Tamanho = table.Column<double>(type: "double precision", nullable: false),
                    EhPrivado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorposHidricos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                schema: "waterPath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Senha = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Coletas",
                schema: "waterPath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ph = table.Column<float>(type: "real", nullable: false),
                    OxigenioDissolvido = table.Column<float>(type: "real", nullable: false),
                    Turbidez = table.Column<float>(type: "real", nullable: false),
                    CloroResidual = table.Column<float>(type: "real", nullable: false),
                    Floretos = table.Column<float>(type: "real", nullable: false),
                    ColiformesTotais = table.Column<float>(type: "real", nullable: false),
                    EscherichiaColi = table.Column<bool>(type: "boolean", nullable: false),
                    CorpoHidricoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coletas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coletas_CorposHidricos_CorpoHidricoId",
                        column: x => x.CorpoHidricoId,
                        principalSchema: "waterPath",
                        principalTable: "CorposHidricos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualidadesFuturas",
                schema: "waterPath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ph = table.Column<float>(type: "real", nullable: false),
                    OxigenioDissolvido = table.Column<float>(type: "real", nullable: false),
                    Turbidez = table.Column<float>(type: "real", nullable: false),
                    CloroResidual = table.Column<float>(type: "real", nullable: false),
                    Floretos = table.Column<float>(type: "real", nullable: false),
                    ColiformesTotais = table.Column<float>(type: "real", nullable: false),
                    EscherichiaColi = table.Column<bool>(type: "boolean", nullable: false),
                    CorpoHidricoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualidadesFuturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualidadesFuturas_CorposHidricos_CorpoHidricoId",
                        column: x => x.CorpoHidricoId,
                        principalSchema: "waterPath",
                        principalTable: "CorposHidricos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Codigos",
                schema: "waterPath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    DataGeracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Usado = table.Column<bool>(type: "boolean", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "UsuarioCorpoHidricos",
                schema: "waterPath",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    CorpoHidricoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioCorpoHidricos", x => new { x.UsuarioId, x.CorpoHidricoId });
                    table.ForeignKey(
                        name: "FK_UsuarioCorpoHidricos_CorposHidricos_CorpoHidricoId",
                        column: x => x.CorpoHidricoId,
                        principalSchema: "waterPath",
                        principalTable: "CorposHidricos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioCorpoHidricos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "waterPath",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CianoBacterias",
                schema: "waterPath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Concentracao = table.Column<float>(type: "real", nullable: false),
                    Unidade = table.Column<string>(type: "text", nullable: false),
                    ColetaId = table.Column<int>(type: "integer", nullable: false),
                    QualidadeFuturaEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CianoBacterias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CianoBacterias_Coletas_ColetaId",
                        column: x => x.ColetaId,
                        principalSchema: "waterPath",
                        principalTable: "Coletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CianoBacterias_QualidadesFuturas_QualidadeFuturaEntityId",
                        column: x => x.QualidadeFuturaEntityId,
                        principalSchema: "waterPath",
                        principalTable: "QualidadesFuturas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Imagens",
                schema: "waterPath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Url = table.Column<string>(type: "text", nullable: false),
                    CorpoHidricoId = table.Column<int>(type: "integer", nullable: false),
                    ColetaId = table.Column<int>(type: "integer", nullable: true),
                    DataUpload = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QualidadeFuturaEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Imagens_Coletas_ColetaId",
                        column: x => x.ColetaId,
                        principalSchema: "waterPath",
                        principalTable: "Coletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Imagens_CorposHidricos_CorpoHidricoId",
                        column: x => x.CorpoHidricoId,
                        principalSchema: "waterPath",
                        principalTable: "CorposHidricos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Imagens_QualidadesFuturas_QualidadeFuturaEntityId",
                        column: x => x.QualidadeFuturaEntityId,
                        principalSchema: "waterPath",
                        principalTable: "QualidadesFuturas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MetaisPesados",
                schema: "waterPath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Concentracao = table.Column<float>(type: "real", nullable: false),
                    Unidade = table.Column<string>(type: "text", nullable: false),
                    ColetaId = table.Column<int>(type: "integer", nullable: false),
                    QualidadeFuturaEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaisPesados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetaisPesados_Coletas_ColetaId",
                        column: x => x.ColetaId,
                        principalSchema: "waterPath",
                        principalTable: "Coletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetaisPesados_QualidadesFuturas_QualidadeFuturaEntityId",
                        column: x => x.QualidadeFuturaEntityId,
                        principalSchema: "waterPath",
                        principalTable: "QualidadesFuturas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Qualidades",
                schema: "waterPath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CorpoHidricoId = table.Column<int>(type: "integer", nullable: false),
                    IQA = table.Column<int>(type: "integer", nullable: false),
                    QualidadeFuturaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Qualidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Qualidades_CorposHidricos_CorpoHidricoId",
                        column: x => x.CorpoHidricoId,
                        principalSchema: "waterPath",
                        principalTable: "CorposHidricos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Qualidades_QualidadesFuturas_QualidadeFuturaId",
                        column: x => x.QualidadeFuturaId,
                        principalSchema: "waterPath",
                        principalTable: "QualidadesFuturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualidadeCianoBacterias",
                schema: "waterPath",
                columns: table => new
                {
                    QualidadeId = table.Column<int>(type: "integer", nullable: false),
                    CianoBacteriaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualidadeCianoBacterias", x => new { x.QualidadeId, x.CianoBacteriaId });
                    table.ForeignKey(
                        name: "FK_QualidadeCianoBacterias_CianoBacterias_CianoBacteriaId",
                        column: x => x.CianoBacteriaId,
                        principalSchema: "waterPath",
                        principalTable: "CianoBacterias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualidadeCianoBacterias_Qualidades_QualidadeId",
                        column: x => x.QualidadeId,
                        principalSchema: "waterPath",
                        principalTable: "Qualidades",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QualidadeMetaisPesados",
                schema: "waterPath",
                columns: table => new
                {
                    QualidadeId = table.Column<int>(type: "integer", nullable: false),
                    MetalPesadoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualidadeMetaisPesados", x => new { x.QualidadeId, x.MetalPesadoId });
                    table.ForeignKey(
                        name: "FK_QualidadeMetaisPesados_MetaisPesados_MetalPesadoId",
                        column: x => x.MetalPesadoId,
                        principalSchema: "waterPath",
                        principalTable: "MetaisPesados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualidadeMetaisPesados_Qualidades_QualidadeId",
                        column: x => x.QualidadeId,
                        principalSchema: "waterPath",
                        principalTable: "Qualidades",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CianoBacterias_ColetaId",
                schema: "waterPath",
                table: "CianoBacterias",
                column: "ColetaId");

            migrationBuilder.CreateIndex(
                name: "IX_CianoBacterias_QualidadeFuturaEntityId",
                schema: "waterPath",
                table: "CianoBacterias",
                column: "QualidadeFuturaEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Codigos_UsuarioId",
                schema: "waterPath",
                table: "Codigos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Coletas_CorpoHidricoId",
                schema: "waterPath",
                table: "Coletas",
                column: "CorpoHidricoId");

            migrationBuilder.CreateIndex(
                name: "IX_Imagens_ColetaId",
                schema: "waterPath",
                table: "Imagens",
                column: "ColetaId");

            migrationBuilder.CreateIndex(
                name: "IX_Imagens_CorpoHidricoId",
                schema: "waterPath",
                table: "Imagens",
                column: "CorpoHidricoId");

            migrationBuilder.CreateIndex(
                name: "IX_Imagens_QualidadeFuturaEntityId",
                schema: "waterPath",
                table: "Imagens",
                column: "QualidadeFuturaEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaisPesados_ColetaId",
                schema: "waterPath",
                table: "MetaisPesados",
                column: "ColetaId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaisPesados_QualidadeFuturaEntityId",
                schema: "waterPath",
                table: "MetaisPesados",
                column: "QualidadeFuturaEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_QualidadeCianoBacterias_CianoBacteriaId",
                schema: "waterPath",
                table: "QualidadeCianoBacterias",
                column: "CianoBacteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_QualidadeMetaisPesados_MetalPesadoId",
                schema: "waterPath",
                table: "QualidadeMetaisPesados",
                column: "MetalPesadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Qualidades_CorpoHidricoId",
                schema: "waterPath",
                table: "Qualidades",
                column: "CorpoHidricoId");

            migrationBuilder.CreateIndex(
                name: "IX_Qualidades_QualidadeFuturaId",
                schema: "waterPath",
                table: "Qualidades",
                column: "QualidadeFuturaId");

            migrationBuilder.CreateIndex(
                name: "IX_QualidadesFuturas_CorpoHidricoId",
                schema: "waterPath",
                table: "QualidadesFuturas",
                column: "CorpoHidricoId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioCorpoHidricos_CorpoHidricoId",
                schema: "waterPath",
                table: "UsuarioCorpoHidricos",
                column: "CorpoHidricoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Codigos",
                schema: "waterPath");

            migrationBuilder.DropTable(
                name: "Imagens",
                schema: "waterPath");

            migrationBuilder.DropTable(
                name: "QualidadeCianoBacterias",
                schema: "waterPath");

            migrationBuilder.DropTable(
                name: "QualidadeMetaisPesados",
                schema: "waterPath");

            migrationBuilder.DropTable(
                name: "UsuarioCorpoHidricos",
                schema: "waterPath");

            migrationBuilder.DropTable(
                name: "CianoBacterias",
                schema: "waterPath");

            migrationBuilder.DropTable(
                name: "MetaisPesados",
                schema: "waterPath");

            migrationBuilder.DropTable(
                name: "Qualidades",
                schema: "waterPath");

            migrationBuilder.DropTable(
                name: "Usuarios",
                schema: "waterPath");

            migrationBuilder.DropTable(
                name: "Coletas",
                schema: "waterPath");

            migrationBuilder.DropTable(
                name: "QualidadesFuturas",
                schema: "waterPath");

            migrationBuilder.DropTable(
                name: "CorposHidricos",
                schema: "waterPath");
        }
    }
}
