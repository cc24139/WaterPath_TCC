using System.Text.Json;
using back_end.src.IA.Domain;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.IA.Application;

public class PredicaoIAService(WaterPathDbContext context, IaClient client)
{
    public const long LimiteImagem = 10 * 1024 * 1024;
    public const string VersaoRegraRisco = "waterpath-risk-v1";

    public async Task<PredicaoIAEntity> CriarAsync(int coletaId, IFormFile image, string? data,
        CancellationToken cancellationToken)
    {
        if (coletaId <= 0) throw new IaException(400, "Informe uma coleta válida.");
        if (image is null || image.Length == 0) throw new IaException(400, "Informe uma imagem.");
        if (image.Length > LimiteImagem) throw new IaException(413, "A imagem deve ter até 10 MB.");
        if (data is not null)
        {
            try
            {
                data = ContratoAmostra.Ler(data).GetRawText();
            }
            catch (JsonException) { throw new IaException(422, "A amostra deve usar os campos, tipos e unidades do contrato da IA, com as quatro medições obrigatórias."); }
        }
        var coleta = await context.Coletas.AsNoTracking().Where(c => c.Id == coletaId)
            .Select(c => new { c.CorpoHidricoId, c.DataHora }).SingleOrDefaultAsync(cancellationToken);
        if (coleta is null)
            throw new IaException(404, "Coleta não encontrada.");

        if (coleta.DataHora == default || coleta.DataHora > DateTimeOffset.UtcNow)
            throw new IaException(422, "A análise atual exige uma coleta com instante válido e não futuro.");
        if (data is not null) data = ContratoAmostra.ReferenciarColeta(data, coleta.DataHora);

        using var stream = new MemoryStream();
        await image.CopyToAsync(stream, cancellationToken);
        var original = stream.ToArray();
        var contentType = IaClient.DetectarContentType(original);
        if (contentType is null) throw new IaException(400, "Envie uma imagem JPEG ou PNG válida.");
        var name = Path.GetFileName(image.FileName);
        if (string.IsNullOrWhiteSpace(name)) name = contentType == "image/png" ? "imagem.png" : "imagem.jpg";
        var history = data is null ? "[]"
            : await BuscarHistoricoAsync(coleta.CorpoHidricoId, coleta.DataHora, cancellationToken);
        var analise = await client.AnalisarAsync(data, original, contentType, name, cancellationToken, history);
        var predicao = new PredicaoIAEntity
        {
            ColetaId = coletaId, CorpoHidricoId = coleta.CorpoHidricoId, DataColeta = coleta.DataHora.UtcDateTime, CriadaEm = DateTime.UtcNow,
            Tipo = data is null ? "vision" : "integrada", EntradaJson = data, ResultadoJson = analise.ResultadoJson,
            NomeArquivo = name, ContentTypeOriginal = contentType, ImagemOriginal = original,
            ContentTypeResultado = "image/jpeg", ImagemResultado = analise.Imagem,
        };
        // Grava imagem e resultado juntos somente após a análise ter sucesso.
        context.PredicoesIA.Add(predicao);
        await context.SaveChangesAsync(cancellationToken);
        return predicao;
    }

    private async Task<string> BuscarHistoricoAsync(int corpoHidricoId, DateTimeOffset dataColeta,
        CancellationToken cancellationToken)
    {
        var coletas = await context.Coletas.AsNoTracking()
            .Where(c => c.CorpoHidricoId == corpoHidricoId && c.DataHora < dataColeta)
            .OrderByDescending(c => c.DataHora).ThenByDescending(c => c.Id)
            .Select(c => c.Id).Take(5).ToListAsync(cancellationToken);
        var predicoes = await context.PredicoesIA.AsNoTracking()
            .Where(p => coletas.Contains(p.ColetaId) && p.Tipo == "integrada" && p.ResultadoJson != null
                && p.CorpoHidricoId == corpoHidricoId && p.DataColeta < dataColeta.UtcDateTime)
            .OrderByDescending(p => p.CriadaEm).ThenByDescending(p => p.Id)
            .Select(p => new { p.Id, p.ColetaId, p.EntradaJson, p.ResultadoJson }).ToListAsync(cancellationToken);
        var history = new List<object>();
        foreach (var id in coletas)
        {
            var latest = predicoes.FirstOrDefault(p => p.ColetaId == id);
            if (latest is null) continue;
            if (!ContratoRisco.Valido(latest.EntradaJson, latest.ResultadoJson!)) continue;
            using var result = JsonDocument.Parse(latest.ResultadoJson!);
            var level = result.RootElement.GetProperty("baseRiskLevel").GetInt32();
            history.Add(new { predictionId = latest.Id, coletaId = id, baseRiskLevel = level });
        }
        return JsonSerializer.Serialize(history);
    }
}
