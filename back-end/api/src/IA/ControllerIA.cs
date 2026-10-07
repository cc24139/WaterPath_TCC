using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using back_end.src.IA.Application;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.IA;

public class PredicaoIAInput
{
    [Range(1, int.MaxValue)] public int ColetaId { get; set; }
    [Required] public IFormFile Image { get; set; } = null!;
    [Required] public string Data { get; set; } = null!;
}

[ApiController]
[Route("api/ia/predicoes")]
public class ControllerIA(WaterPathDbContext context, PredicaoIAService service) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(PredicaoIAService.LimiteImagem + 1024 * 1024)]
    public async Task<IActionResult> Predizer([FromForm] PredicaoIAInput input, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.CriarAsync(input.ColetaId, input.Image, input.Data, cancellationToken);
            return CreatedAtAction(nameof(Obter), new { id = result.Id }, new
            {
                result.Id, result.ColetaId, result.CorpoHidricoId, result.DataColeta, result.Tipo, result.CriadaEm,
                Resultado = JsonSerializer.Deserialize<JsonElement>(result.ResultadoJson!),
                ImagemOriginalUrl = $"/api/ia/predicoes/{result.Id}/imagem/original",
                ImagemResultadoUrl = $"/api/ia/predicoes/{result.Id}/imagem",
            });
        }
        catch (IaException ex) { return Problem(statusCode: ex.StatusCode, detail: ex.Message); }
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int? coletaId, CancellationToken cancellationToken,
        [FromQuery, Range(1, int.MaxValue)] int pagina = 1, [FromQuery, Range(1, 100)] int tamanhoPagina = 20)
    {
        var query = context.PredicoesIA.AsNoTracking();
        if (coletaId.HasValue) query = query.Where(p => p.ColetaId == coletaId.Value);
        var total = await query.CountAsync(cancellationToken);
        var itens = await query.OrderByDescending(p => p.CriadaEm).ThenByDescending(p => p.Id)
            .Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina)
            .Select(p => new { p.Id, p.ColetaId, p.CorpoHidricoId, p.DataColeta, p.Tipo, p.CriadaEm }).ToListAsync(cancellationToken);
        return Ok(new { total, pagina, tamanhoPagina, itens });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id, CancellationToken cancellationToken)
    {
        var p = await context.PredicoesIA.AsNoTracking().Where(p => p.Id == id)
            .Select(p => new { p.Id, p.ColetaId, p.CorpoHidricoId, p.DataColeta, p.Tipo, p.CriadaEm, p.EntradaJson, p.ResultadoJson })
            .SingleOrDefaultAsync(cancellationToken);
        if (p is null) return NotFound("Predição não encontrada.");
        return Ok(new
        {
            p.Id, p.ColetaId, p.CorpoHidricoId, p.DataColeta, p.Tipo, p.CriadaEm,
            Entrada = p.EntradaJson is null ? (JsonElement?)null : JsonSerializer.Deserialize<JsonElement>(p.EntradaJson),
            Resultado = p.ResultadoJson is null ? (JsonElement?)null : JsonSerializer.Deserialize<JsonElement>(p.ResultadoJson),
            ImagemOriginalUrl = $"/api/ia/predicoes/{p.Id}/imagem/original",
            ImagemResultadoUrl = $"/api/ia/predicoes/{p.Id}/imagem",
        });
    }

    [HttpGet("{id:int}/imagem")]
    public async Task<IActionResult> Imagem(int id, CancellationToken cancellationToken)
    {
        var p = await context.PredicoesIA.AsNoTracking().Where(p => p.Id == id)
            .Select(p => new { p.ImagemResultado, p.ContentTypeResultado }).SingleOrDefaultAsync(cancellationToken);
        return p is null ? NotFound("Predição não encontrada.") : File(p.ImagemResultado, p.ContentTypeResultado, "predicao.jpg");
    }

    [HttpGet("{id:int}/imagem/original")]
    public async Task<IActionResult> ImagemOriginal(int id, CancellationToken cancellationToken)
    {
        var p = await context.PredicoesIA.AsNoTracking().Where(p => p.Id == id)
            .Select(p => new { p.ImagemOriginal, p.ContentTypeOriginal, p.NomeArquivo }).SingleOrDefaultAsync(cancellationToken);
        return p is null ? NotFound("Predição não encontrada.") : File(p.ImagemOriginal, p.ContentTypeOriginal, p.NomeArquivo);
    }
}
