using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace WaterPath.Api.Tests;

public class MigrationContractTests
{
    [Fact]
    public void CadastroAnalisePreservaLegadosEUsaPrecisaoDupla()
    {
        using var db = new WaterPathDbContext(new DbContextOptionsBuilder<WaterPathDbContext>()
            .UseNpgsql("Host=localhost;Database=waterpath_contract_test").Options);
        var sql = db.GetService<IMigrator>().GenerateScript("20261007120000_ReferenciarDadosClassificacao",
            "20261008140000_CadastroAnalise");
        Assert.Contains("ADD \"ResponsavelId\" integer", sql);
        Assert.Contains("double precision", sql);
        Assert.DoesNotContain("UPDATE ", sql);
        Assert.DoesNotContain("DROP TABLE", sql);
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public void MigracaoAdicionaReferenciasNulasSemReescreverDadosLegados()
    {
        using var db = new WaterPathDbContext(new DbContextOptionsBuilder<WaterPathDbContext>()
            .UseNpgsql("Host=localhost;Database=waterpath_contract_test").Options);
        var sql = db.GetService<IMigrator>().GenerateScript("20261004181139_PersistirPredicoesIA",
            "20261007120000_ReferenciarDadosClassificacao");
        Assert.Contains("ADD \"CorpoHidricoId\" integer", sql);
        Assert.Contains("ADD \"DataColeta\" timestamp with time zone", sql);
        Assert.DoesNotContain("DROP TABLE", sql);
        Assert.DoesNotContain("UPDATE \"waterPath\".\"PredicoesIA\"", sql);
        Assert.DoesNotContain("DELETE FROM", sql);
        Assert.False(db.Database.HasPendingModelChanges());
    }
}
