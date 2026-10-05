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
                using var document = JsonDocument.Parse(data);
                if (document.RootElement.ValueKind != JsonValueKind.Object) throw new JsonException();
                data = document.RootElement.GetRawText();
            }
            catch (JsonException) { throw new IaException(400, "O campo data deve ser um objeto JSON válido."); }
        }
        var coleta = await context.Coletas.AsNoTracking().Where(c => c.Id == coletaId)
            .Select(c => new { c.CorpoHidricoId, c.DataHora }).SingleOrDefaultAsync(cancellationToken);
        if (coleta is null)
            throw new IaException(404, "Coleta não encontrada.");

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
            ColetaId = coletaId, CriadaEm = DateTime.UtcNow,
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
            .Where(p => coletas.Contains(p.ColetaId) && p.Tipo == "integrada" && p.ResultadoJson != null)
            .OrderByDescending(p => p.CriadaEm).ThenByDescending(p => p.Id)
            .Select(p => new { p.Id, p.ColetaId, p.ResultadoJson }).ToListAsync(cancellationToken);
        var history = new List<object>();
        foreach (var id in coletas)
        {
            var latest = predicoes.FirstOrDefault(p => p.ColetaId == id);
            if (latest is null) continue;
            using var result = JsonDocument.Parse(latest.ResultadoJson!);
            var root = result.RootElement;
            // Análises antigas sem esta regra não contam como ocorrência.
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("riskRuleVersion", out var version) || version.ValueKind != JsonValueKind.String
                || version.GetString() != VersaoRegraRisco
                || !root.TryGetProperty("baseRiskLevel", out var basis) || basis.ValueKind != JsonValueKind.Number
                || !basis.TryGetInt32(out var level) || level is < 1 or > 3)
                continue;
            history.Add(new { predictionId = latest.Id, coletaId = id, baseRiskLevel = level });
        }
        return JsonSerializer.Serialize(history);
    }
}
