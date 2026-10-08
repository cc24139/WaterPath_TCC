using System.Text.Json;
using back_end.src.Domain.Coleta;
using back_end.src.Medicoes.Domain;

namespace back_end.src.IA.Application;

public static class AmostraDaColeta
{
    public static string Montar(ColetaEntity coleta)
    {
        var sample = new Dictionary<string, object?>
        {
            ["data"] = coleta.DataHora.ToUniversalTime().ToString("O"),
            ["profundidade"] = coleta.ProfundidadeMetros,
        };
        Adicionar(Medicao.Temperatura, "temperatura", "°c", true);
        Adicionar(Medicao.Ph, "ph", "ph", true);
        Adicionar(Medicao.CondutividadeEletrica, "condutividade_eletrica", "us/cm", true);
        Adicionar(Medicao.OxigenioDissolvido, "oxigenio_dissolvido", "mg/l", true);
        Adicionar(Medicao.SolidosSuspensosTotais, "solidos_suspensos_totais", "mg/l", false);
        Adicionar(Medicao.CarbonoOrganicoTotal, "carbono_organico_total", "mg/l", false);
        Adicionar(Medicao.FosforoTotal, "fosforo_total", "ug/l", false);
        if (coleta.MetaisPesados.Count > 0)
            sample["metais_pesados"] = coleta.MetaisPesados.OrderBy(m => m.Id)
                .Select(m => new { name = m.Nome, value = m.Concentracao, unit = m.Unidade }).ToArray();
        try { return ContratoAmostra.Ler(JsonSerializer.Serialize(sample)).GetRawText(); }
        catch (Exception ex) when (ex is JsonException or ArgumentException)
        { throw new IaException(422, "As medições salvas da coleta não atendem ao contrato da IA."); }

        void Adicionar(Medicao codigo, string field, string unit, bool required)
        {
            var records = coleta.Medicoes.Where(m => m.codigoMedicao == codigo).ToArray();
            if (records.Length > 1) throw new IaException(422, $"Medição duplicada: {codigo}.");
            var record = records.SingleOrDefault();
            if (record is null || record.censurado || record.valor is null)
            {
                if (required) throw new IaException(422, $"A coleta exige uma medição não censurada de {codigo}.");
                sample[field] = null;
                return;
            }
            var actualUnit = record.unidade.Trim().ToLowerInvariant().Replace('µ', 'u').Replace('μ', 'u');
            var value = record.valor.Value;
            // O banco usa mg/L para fósforo; a regressão espera µg P/L.
            if (codigo == Medicao.FosforoTotal && actualUnit == "mg/l") value *= 1000;
            else if (actualUnit != unit) throw new IaException(422, $"Unidade incompatível para {codigo}: {record.unidade}.");
            if (!double.IsFinite(value)) throw new IaException(422, $"Valor inválido para {codigo}.");
            sample[field] = value;
        }
    }

    // Observações históricas mantêm unidades, ausências e censura originais.
    public static object Retrato(ColetaEntity coleta) => new
    {
        waterBodyId = coleta.CorpoHidricoId, collectionId = coleta.Id,
        collectedAt = coleta.DataHora.ToUniversalTime().ToString("O"),
        measurements = coleta.Medicoes.OrderBy(m => m.codigoMedicao).Select(m => new
        { code = m.codigoMedicao.ToString(), value = m.valor, unit = m.unidade, censored = m.censurado, limit = m.limite }),
        metals = coleta.MetaisPesados.OrderBy(m => m.Id).Select(m => new
        { name = m.Nome, value = m.Concentracao, unit = m.Unidade }),
    };
}
