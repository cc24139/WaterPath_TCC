using System.Text.Json;
using back_end.src.IA.Domain;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.IA.Application;

public class PredicaoIAService(WaterPathDbContext context, IaClient client)
{
    public const long LimiteImagem = 10 * 1024 * 1024;

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
        if (!await context.Coletas.AnyAsync(c => c.Id == coletaId, cancellationToken))
            throw new IaException(404, "Coleta não encontrada.");

        using var stream = new MemoryStream();
        await image.CopyToAsync(stream, cancellationToken);
        var original = stream.ToArray();
        var contentType = IaClient.DetectarContentType(original);
        if (contentType is null) throw new IaException(400, "Envie uma imagem JPEG ou PNG válida.");
        var name = Path.GetFileName(image.FileName);
        if (string.IsNullOrWhiteSpace(name)) name = contentType == "image/png" ? "imagem.png" : "imagem.jpg";
        var resultado = data is null ? null
            : await client.PredizerAsync(data, original, contentType, name, cancellationToken);
        var anotada = await client.PredizerImagemAsync(original, contentType, name, cancellationToken);
        var predicao = new PredicaoIAEntity
        {
            ColetaId = coletaId, CriadaEm = DateTime.UtcNow,
            Tipo = data is null ? "vision" : "integrada", EntradaJson = data, ResultadoJson = resultado,
            NomeArquivo = name, ContentTypeOriginal = contentType, ImagemOriginal = original,
            ContentTypeResultado = anotada.ContentType, ImagemResultado = anotada.Bytes,
        };
        // Um único SaveChanges grava tudo em uma transação, após as duas chamadas terem sucesso.
        context.PredicoesIA.Add(predicao);
        await context.SaveChangesAsync(cancellationToken);
        return predicao;
    }
}
