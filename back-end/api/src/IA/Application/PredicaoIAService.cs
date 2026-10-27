using System.Text.Json;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.Imagem;
using back_end.src.Domain.QualidadeFutura;
using back_end.src.Medicoes.Domain;
using back_end.src.IA.Domain;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace back_end.src.IA.Application;

public class PredicaoIAService
{
    private readonly WaterPathDbContext context;
    private readonly IaClient client;
    private readonly ILogger<PredicaoIAService> logger;
    private readonly IWebHostEnvironment? environment;

    public PredicaoIAService(WaterPathDbContext context, IaClient client,
        ILogger<PredicaoIAService>? logger = null, IWebHostEnvironment? environment = null)
    {
        this.context = context;
        this.client = client;
        this.logger = logger ?? NullLogger<PredicaoIAService>.Instance;
        this.environment = environment;
    }

    public const long LimiteImagem = 10 * 1024 * 1024;
    public const string VersaoRegraRisco = "waterpath-risk-v1";

    public async Task<PredicaoIAEntity> AnalisarLagoAsync(int corpoHidricoId, IFormFile image,
        CancellationToken cancellationToken)
    {
        if (corpoHidricoId <= 0) throw new IaException(400, "Informe um ID de corpo hídrico válido.");
        if (!await context.CorposHidricos.AsNoTracking().AnyAsync(c => c.Id == corpoHidricoId, cancellationToken))
            throw new IaException(404, "Corpo hídrico não encontrado.");
        var coletas = await context.Coletas.AsNoTracking()
            .Where(c => c.CorpoHidricoId == corpoHidricoId)
            .Include(c => c.Medicoes).Include(c => c.MetaisPesados)
            .AsSingleQuery()
            .OrderBy(c => c.DataHora).ThenBy(c => c.Id).ToListAsync(cancellationToken);
        var atual = coletas.LastOrDefault();
        if (atual is null) throw new IaException(422, "Corpo hídrico sem coletas para analisar.");
        var collectionContext = SerializarContextoColetas(corpoHidricoId, atual.Id, atual.DataHora,
            coletas.Where(c => c.DataHora < atual.DataHora));
        return await ProcessarAsync(atual.Id, await LerUploadAsync(image, cancellationToken),
            AmostraDaColeta.Montar(atual), cancellationToken, collectionContext, atual);
    }

    public async Task<PredicaoIAEntity> CriarAsync(int coletaId, IFormFile? image,
        CancellationToken cancellationToken, Guid? requestKey = null, string? requestHash = null)
    {
        if (coletaId <= 0) throw new IaException(400, "Informe uma coleta válida.");
        var coleta = await context.Coletas.AsNoTracking().Where(c => c.Id == coletaId)
            .Include(c => c.Medicoes).Include(c => c.MetaisPesados).SingleOrDefaultAsync(cancellationToken);
        if (coleta is null) throw new IaException(404, "Coleta não encontrada.");
        var data = AmostraDaColeta.Montar(coleta);
        var payload = image is null
            ? await ObterImagemDaColetaAsync(coleta, cancellationToken)
            : await LerUploadAsync(image, cancellationToken);
        return await ProcessarAsync(coletaId, payload, data, cancellationToken,
            coletaSnapshot: coleta, requestKey: requestKey, requestHash: requestHash, salvarQualidadeFutura: true);
    }

    private sealed record ImagePayload(byte[] Bytes, string FileName);

    private static async Task<ImagePayload> LerUploadAsync(IFormFile image, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0) throw new IaException(400, "A imagem enviada está vazia.");
        if (image.Length > LimiteImagem) throw new IaException(413, "A imagem deve ter até 10 MB.");
        using var stream = new MemoryStream();
        await image.CopyToAsync(stream, cancellationToken);
        var name = Path.GetFileName(image.FileName);
        return new ImagePayload(stream.ToArray(), string.IsNullOrWhiteSpace(name) ? "imagem" : name);
    }

    private async Task<ImagePayload> ObterImagemDaColetaAsync(ColetaEntity coleta, CancellationToken cancellationToken)
    {
        var vinculadas = await context.Imagens.AsNoTracking()
            .Where(i => i.Coleta != null && i.Coleta.Id == coleta.Id && i.CorpoHidrico.Id == coleta.CorpoHidricoId)
            .OrderByDescending(i => i.DataUpload).ThenByDescending(i => i.Id)
            .Select(i => new { i.Url, i.Id }).ToListAsync(cancellationToken);
        var doCorpo = await context.Imagens.AsNoTracking()
            .Where(i => i.CorpoHidrico.Id == coleta.CorpoHidricoId)
            .OrderByDescending(i => i.DataUpload).ThenByDescending(i => i.Id)
            .Select(i => new { i.Url, i.Id }).ToListAsync(cancellationToken);
        foreach (var candidate in vinculadas.Concat(doCorpo).DistinctBy(i => i.Id))
        {
            var path = ResolverCaminhoImagem(candidate.Url);
            try
            {
                if (new FileInfo(path).Length > LimiteImagem)
                    throw new IaException(413, "A imagem armazenada deve ter até 10 MB.");
                var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
                if (bytes.Length == 0) continue;
                var contentType = IaClient.DetectarContentType(bytes);
                if (contentType is null) continue;
                return new ImagePayload(bytes, Path.GetFileName(path));
            }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
            catch (UnauthorizedAccessException) { }
        }
        throw new IaException(422, "Nenhuma imagem JPEG ou PNG disponível para a coleta ou seu corpo hídrico.");
    }

    private string ResolverCaminhoImagem(string url)
    {
        var root = Path.GetFullPath(environment?.ContentRootPath ?? Directory.GetCurrentDirectory());
        var path = Path.GetFullPath(url, root);
        var relative = Path.GetRelativePath(root, path);
        if (Path.IsPathRooted(relative) || relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar,
                StringComparison.Ordinal))
            throw new IaException(422, "O caminho da imagem armazenada é inválido.");
        return path;
    }

    private async Task<PredicaoIAEntity> ProcessarAsync(int coletaId, ImagePayload image, string? data,
        CancellationToken cancellationToken, string? collectionContext = null, ColetaEntity? coletaSnapshot = null,
        Guid? requestKey = null, string? requestHash = null, bool salvarQualidadeFutura = false)
    {
        if (coletaId <= 0) throw new IaException(400, "Informe uma coleta válida.");
        if (image.Bytes.Length == 0) throw new IaException(400, "Informe uma imagem.");
        if (image.Bytes.Length > LimiteImagem) throw new IaException(413, "A imagem deve ter até 10 MB.");
        if (data is not null)
        {
            try
            {
                data = ContratoAmostra.Ler(data).GetRawText();
            }
            catch (JsonException) { throw new IaException(422, "A amostra deve usar os campos, tipos e unidades do contrato da IA, com as quatro medições obrigatórias."); }
        }
        var coleta = coletaSnapshot is not null
            ? new { coletaSnapshot.CorpoHidricoId, coletaSnapshot.DataHora }
            : await context.Coletas.AsNoTracking().Where(c => c.Id == coletaId)
            .Select(c => new { c.CorpoHidricoId, c.DataHora }).SingleOrDefaultAsync(cancellationToken);
        if (coleta is null)
            throw new IaException(404, "Coleta não encontrada.");

        if (coleta.DataHora == default || coleta.DataHora > DateTimeOffset.UtcNow)
            throw new IaException(422, "A análise atual exige uma coleta com instante válido e não futuro.");
        if (data is not null) data = ContratoAmostra.ReferenciarColeta(data, coleta.DataHora);
        if (data is not null && collectionContext is null)
            collectionContext = await BuscarContextoColetasAsync(coletaId, coleta.CorpoHidricoId,
                coleta.DataHora, cancellationToken);

        var original = image.Bytes;
        var contentType = IaClient.DetectarContentType(original);
        if (contentType is null) throw new IaException(400, "Envie uma imagem JPEG ou PNG válida.");
        var name = image.FileName;
        if (string.IsNullOrWhiteSpace(name)) name = contentType == "image/png" ? "imagem.png" : "imagem.jpg";
        var history = data is null ? "[]"
            : await BuscarHistoricoAsync(coleta.CorpoHidricoId, coleta.DataHora, cancellationToken);
        logger.LogInformation(
            "Iniciando análise de IA. ColetaId={CollectionId} CorpoHidricoId={WaterBodyId} Tipo={AnalysisType} TamanhoImagem={ImageBytes}",
            coletaId, coleta.CorpoHidricoId, data is null ? "vision" : "integrada", original.Length);
        var analise = await client.AnalisarAsync(data, original, contentType, name, cancellationToken, history, collectionContext);
        var predicao = new PredicaoIAEntity
        {
            ColetaId = coletaId, CorpoHidricoId = coleta.CorpoHidricoId, DataColeta = coleta.DataHora.UtcDateTime, CriadaEm = DateTime.UtcNow,
            Tipo = data is null ? "vision" : "integrada", EntradaJson = data, ResultadoJson = analise.ResultadoJson,
            AnaliseRequestKey = requestKey, AnaliseRequestHash = requestHash,
            NomeArquivo = name, ContentTypeOriginal = contentType, ImagemOriginal = original,
            ContentTypeResultado = "image/jpeg", ImagemResultado = analise.Imagem,
        };
        QualidadeFuturaEntity? qualidadeFutura = null;
        if (salvarQualidadeFutura)
        {
            using var resultado = JsonDocument.Parse(analise.ResultadoJson);
            var root = resultado.RootElement;
            var qualityFields = new[]
            {
                "detections", "totalObjects", "metalPredictions", "measuredMetals", "estimatedMetals",
                "riskLevel", "riskLabel", "riskReasons", "metalVariation",
            }.Where(name => root.TryGetProperty(name, out _))
                .ToDictionary(name => name, name => root.GetProperty(name).Clone());
            var collectionDate = coletaSnapshot!.DataHora;
            qualidadeFutura = new QualidadeFuturaEntity
            {
                CorpoHidricoId = coletaSnapshot.CorpoHidricoId,
                ColetaId = coletaSnapshot.Id,
                DataReferencia = DateOnly.FromDateTime(collectionDate.UtcDateTime),
                ResultadoJson = JsonSerializer.Serialize(qualityFields),
                CriadaEm = DateTime.UtcNow,
            };
        }
        // Grava imagem e resultado juntos somente após a análise ter sucesso.
        context.PredicoesIA.Add(predicao);
        if (qualidadeFutura is not null) context.QualidadesFuturas.Add(qualidadeFutura);
        try { await context.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) when (requestKey.HasValue)
        {
            context.Entry(predicao).State = EntityState.Detached;
            if (qualidadeFutura is not null) context.Entry(qualidadeFutura).State = EntityState.Detached;
            var prior = await context.PredicoesIA.AsNoTracking()
                .SingleOrDefaultAsync(p => p.AnaliseRequestKey == requestKey.Value, cancellationToken);
            if (prior is not null && prior.ColetaId == coletaId && prior.AnaliseRequestHash == requestHash)
                return prior;
            if (prior is not null)
                throw new IaException(409, "Esta chave de requisição já foi usada com outros dados.");
            throw new IaException(500, IaErrorCodes.Persistence, "Não foi possível salvar a análise.");
        }
        catch (DbUpdateException)
        {
            // Impede que uma gravação posterior no mesmo contexto tente salvar esta análise falha.
            context.Entry(predicao).State = EntityState.Detached;
            if (qualidadeFutura is not null) context.Entry(qualidadeFutura).State = EntityState.Detached;
            throw new IaException(500, IaErrorCodes.Persistence, "Não foi possível salvar a análise.");
        }
        catch (OperationCanceledException)
        {
            context.Entry(predicao).State = EntityState.Detached;
            if (qualidadeFutura is not null) context.Entry(qualidadeFutura).State = EntityState.Detached;
            throw;
        }
        logger.LogInformation(
            "Análise de IA concluída e persistida. PredicaoId={PredictionId} ColetaId={CollectionId} Tipo={AnalysisType}",
            predicao.Id, predicao.ColetaId, predicao.Tipo);
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
        foreach (var id in coletas.AsEnumerable().Reverse())
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

    private async Task<string> BuscarContextoColetasAsync(int currentCollectionId, int waterBodyId,
        DateTimeOffset collectedAt, CancellationToken cancellationToken)
    {
        var previousCollections = await context.Coletas.AsNoTracking()
            .Where(c => c.CorpoHidricoId == waterBodyId && c.Id != currentCollectionId && c.DataHora < collectedAt)
            .Include(c => c.Medicoes).Include(c => c.MetaisPesados)
            .AsSingleQuery()
            .OrderBy(c => c.DataHora).ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);
        return SerializarContextoColetas(waterBodyId, currentCollectionId, collectedAt, previousCollections);
    }

    private static string SerializarContextoColetas(int waterBodyId, int currentCollectionId,
        DateTimeOffset currentCollectedAt, IEnumerable<ColetaEntity> history) => JsonSerializer.Serialize(new
    {
        waterBodyId,
        currentCollectionId,
        currentCollectedAt = currentCollectedAt.ToUniversalTime().ToString("O"),
        history = history.Where(c => c.CorpoHidricoId == waterBodyId && c.Id != currentCollectionId
                && c.DataHora < currentCollectedAt)
            .OrderBy(c => c.DataHora).ThenBy(c => c.Id)
            .Select(AmostraDaColeta.Retrato),
    });
}
