using System.Text.Json;
using back_end.src.Domain.Coleta;
using back_end.src.IA.Application;
using back_end.src.Medicoes.Domain;
using Xunit;

namespace WaterPath.Api.Tests;

public class AmostraDaColetaTests
{
    [Fact]
    public void MontarNormalizaUnidadesConverteFosforoERepresentaOpcionaisAusentes()
    {
        var coleta = new ColetaEntity(1, DateTimeOffset.Parse("2026-10-01T12:00:00Z"), null, null, 2);
        coleta.AdicionarMedicao(new(Medicao.Temperatura, 20, "°C", false, null));
        coleta.AdicionarMedicao(new(Medicao.Ph, 7, "pH", false, null));
        coleta.AdicionarMedicao(new(Medicao.CondutividadeEletrica, 100, "µS/cm", false, null));
        coleta.AdicionarMedicao(new(Medicao.OxigenioDissolvido, 8, "mg/L", false, null));
        coleta.AdicionarMedicao(new(Medicao.FosforoTotal, 0.02, "mg/L", false, null));

        using var sample = JsonDocument.Parse(AmostraDaColeta.Montar(coleta));
        var root = sample.RootElement;

        Assert.Equal(20, root.GetProperty("temperatura").GetDouble());
        Assert.Equal(100, root.GetProperty("condutividade_eletrica").GetDouble());
        Assert.Equal(20, root.GetProperty("fosforo_total").GetDouble());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("solidos_suspensos_totais").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("carbono_organico_total").ValueKind);
    }

    [Fact]
    public void MontarRejeitaMedicaoObrigatoriaAusente()
    {
        var coleta = new ColetaEntity(1, DateTimeOffset.UtcNow, null, null, null);

        var error = Assert.Throws<IaException>(() => AmostraDaColeta.Montar(coleta));

        Assert.Equal(422, error.StatusCode);
        Assert.Contains("Temperatura", error.Message);
    }
}
