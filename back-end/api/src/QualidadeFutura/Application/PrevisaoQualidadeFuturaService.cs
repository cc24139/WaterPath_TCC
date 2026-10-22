using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.QualidadeFutura;
using back_end.src.IA.Application;
using back_end.src.Medicoes.Domain;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.QualidadeFutura.Application;

public sealed record CriarPrevisaoInput(
    int CorpoHidricoId, int HorizonteDias, double? Latitude = null,
    double? Longitude = null, double? ProfundidadeMetros = null);

public sealed class PrevisaoQualidadeFuturaService(
    WaterPathDbContext context, HttpClient client, IConfiguration configuration)
{
    private static readonly IReadOnlyDictionary<Medicao, string> Parametros =
        new Dictionary<Medicao, string>
        {
            [Medicao.Temperatura] = "temperatura",
            [Medicao.Ph] = "ph",
            [Medicao.CondutividadeEletrica] = "condutividade_eletrica",
            [Medicao.OxigenioDissolvido] = "oxigenio_dissolvido",
            [Medicao.SolidosSuspensosTotais] = "solidos_suspensos_totais",
            [Medicao.CarbonoOrganicoTotal] = "carbono_organico_total",
            [Medicao.FosforoTotal] = "fosforo_total",
        };

    public async Task<QualidadeFuturaEntity> CriarAsync(
        CriarPrevisaoInput input, CancellationToken cancellationToken)
    {
        if (input.CorpoHidricoId <= 0 || input.HorizonteDias is not (7 or 15))
            throw new IaException(400, "Informe corpo hídrico válido e horizonte de 7 ou 15 dias.");
        if (input.Latitude.HasValue != input.Longitude.HasValue ||
            new[] { input.Latitude, input.Longitude, input.ProfundidadeMetros }
                .Any(value => value.HasValue && !double.IsFinite(value.Value)) ||
            input.Latitude is < -90 or > 90 || input.Longitude is < -180 or > 180 ||
            input.ProfundidadeMetros is < 0)
            throw new IaException(400, "Informe coordenadas e profundidade válidas.");

        if (!await context.CorposHidricos.AsNoTracking()
                .AnyAsync(c => c.Id == input.CorpoHidricoId, cancellationToken))
            throw new IaException(404, "Corpo hídrico não encontrado.");

        var instanteAtual = DateTimeOffset.UtcNow;
        var coletas = await context.Coletas.AsNoTracking()
            .Where(c => c.CorpoHidricoId == input.CorpoHidricoId && c.DataHora <= instanteAtual)
            .Include(c => c.Medicoes)
            .OrderBy(c => c.DataHora).ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);
        if (input.Latitude.HasValue)
            coletas = coletas.Where(c => c.Latitude == input.Latitude && c.Longitude == input.Longitude).ToList();
        if (input.ProfundidadeMetros.HasValue)
            coletas = coletas.Where(c => c.ProfundidadeMetros == input.ProfundidadeMetros).ToList();
        if (coletas.Count == 0)
            throw new IaException(422, "Não há coletas no contexto informado.");

        var contextos = coletas.GroupBy(c => (c.Latitude, c.Longitude, c.ProfundidadeMetros)).ToList();
        if (contextos.Count != 1)
            throw new IaException(422,
                "Há coletas em pontos ou profundidades diferentes. Informe latitude, longitude e profundidade para selecionar um contexto.");
        var selecionadas = contextos[0].ToList();
        var contexto = contextos[0].Key;
        var ponto = Ponto(contexto.Latitude, contexto.Longitude);
        var ultimaDataHora = selecionadas[^1].DataHora.UtcDateTime;
        var ultimaData = DateOnly.FromDateTime(ultimaDataHora);
        var rota = configuration["IA:PredictGeralPath"];
        if (string.IsNullOrWhiteSpace(rota) || !Uri.TryCreate(rota, UriKind.Relative, out var relative)
            || rota.StartsWith('/') || rota.StartsWith("//") || rota.Contains('?') || rota.Contains('#'))
            throw new IaException(503, IaErrorCodes.Unavailable,
                "A rota de previsão geral da IA não está configurada no servidor.");

        var historico = selecionadas.Select(c => MontarColeta(c, ponto)).ToArray();
        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync(relative, new
            {
                coletas = historico,
                horizonte_dias = input.HorizonteDias,
                ponto,
                profundidade = contexto.ProfundidadeMetros,
            }, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new IaException(503, IaErrorCodes.Unavailable, "O serviço de previsão da IA está indisponível.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new IaException(504, IaErrorCodes.Timeout, "O serviço de previsão da IA excedeu o tempo de resposta.");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity)
                    throw new IaException(422, IaErrorCodes.InvalidInput,
                        "O serviço de previsão rejeitou o histórico de coletas.");
                throw new IaException(503, IaErrorCodes.Unavailable,
                    "O serviço de previsão da IA está indisponível.");
            }
            string json;
            try { json = await response.Content.ReadAsStringAsync(cancellationToken); }
            catch (HttpRequestException)
            {
                throw new IaException(503, IaErrorCodes.Unavailable,
                    "Não foi possível ler a resposta do serviço de previsão.");
            }
            ValidarResposta(json, ultimaDataHora, input.HorizonteDias, ponto, contexto.ProfundidadeMetros);
            var previsao = new QualidadeFuturaEntity
            {
                CorpoHidricoId = input.CorpoHidricoId,
                DataReferencia = ultimaData,
                DataAlvo = ultimaData.AddDays(input.HorizonteDias),
                HorizonteDias = input.HorizonteDias,
                Ponto = ponto,
                Profundidade = contexto.ProfundidadeMetros,
                ResultadoJson = json,
                CriadaEm = DateTime.UtcNow,
            };
            context.QualidadesFuturas.Add(previsao);
            try { await context.SaveChangesAsync(cancellationToken); }
            catch (DbUpdateException)
            {
                context.Entry(previsao).State = EntityState.Detached;
                throw new IaException(500, IaErrorCodes.Persistence, "Não foi possível salvar a previsão.");
            }
            return previsao;
        }
    }

    public Task<QualidadeFuturaEntity?> ObterPorIdAsync(int id, CancellationToken cancellationToken)
        => context.QualidadesFuturas.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id && p.ResultadoJson != null, cancellationToken);

    public Task<List<QualidadeFuturaEntity>> ListarAsync(
        int corpoHidricoId, int? horizonteDias, CancellationToken cancellationToken)
    {
        if (corpoHidricoId <= 0 || horizonteDias is not (null or 7 or 15))
            throw new IaException(400, "Informe corpo hídrico válido e horizonte de 7 ou 15 dias.");
        return context.QualidadesFuturas.AsNoTracking()
            .Where(p => p.ResultadoJson != null && p.CorpoHidricoId == corpoHidricoId &&
                (!horizonteDias.HasValue || p.HorizonteDias == horizonteDias.Value))
            .OrderByDescending(p => p.CriadaEm).ThenByDescending(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    private static string? Ponto(double? latitude, double? longitude)
        => latitude.HasValue && longitude.HasValue
            ? $"{latitude.Value.ToString("G17", CultureInfo.InvariantCulture)},{longitude.Value.ToString("G17", CultureInfo.InvariantCulture)}"
            : null;

    private static Dictionary<string, object?> MontarColeta(ColetaEntity coleta, string? ponto)
    {
        var result = new Dictionary<string, object?>
        {
            ["data"] = coleta.DataHora.ToUniversalTime().ToString("O"),
            ["ponto"] = ponto,
            ["profundidade"] = coleta.ProfundidadeMetros,
        };
        foreach (var parametro in Parametros)
        {
            var medicao = coleta.Medicoes.SingleOrDefault(m => m.codigoMedicao == parametro.Key);
            result[parametro.Value] = medicao is { censurado: false, valor: not null }
                ? new { valor = medicao.valor, unidade = medicao.unidade }
                : null;
        }
        return result;
    }

    private static void ValidarResposta(string json, DateTime referencia, int horizonte,
        string? ponto, double? profundidade)
    {
        try
        {
            if (json.Length > 262_144) throw new JsonException();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !DateTime.TryParse(root.GetProperty("data_referencia").GetString(),
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dataReferencia) ||
                !DateTime.TryParse(root.GetProperty("data_alvo").GetString(),
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dataAlvo) ||
                Math.Abs((dataReferencia - referencia).TotalMilliseconds) >= 0.001 ||
                Math.Abs((dataAlvo - referencia.AddDays(horizonte)).TotalMilliseconds) >= 0.001 ||
                root.GetProperty("horizonte_dias").GetInt32() != horizonte ||
                root.GetProperty("ponto").GetString() != ponto ||
                (root.GetProperty("profundidade").ValueKind == JsonValueKind.Null
                    ? (double?)null : root.GetProperty("profundidade").GetDouble()) != profundidade)
                throw new JsonException();
            var parametros = root.GetProperty("parametros");
            if (parametros.ValueKind != JsonValueKind.Object) throw new JsonException();
            foreach (var nome in Parametros.Values)
            {
                var item = parametros.GetProperty(nome);
                var valor = item.GetProperty("valor");
                if (valor.ValueKind is not (JsonValueKind.Null or JsonValueKind.Number) ||
                    valor.ValueKind == JsonValueKind.Number && !double.IsFinite(valor.GetDouble()) ||
                    item.GetProperty("unidade").ValueKind != JsonValueKind.String ||
                    item.GetProperty("evidencia").ValueKind != JsonValueKind.String)
                    throw new JsonException();
            }
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            throw new IaException(502, IaErrorCodes.InvalidResponse,
                "O serviço de IA retornou uma previsão geral inválida.");
        }
    }
}
