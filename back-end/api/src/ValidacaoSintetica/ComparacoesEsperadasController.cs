using System.Security.Claims;
using back_end.src.Authorization;
using back_end.src.ValidacaoSintetica.Domain;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.ValidacaoSintetica;

public sealed class ComparacaoEsperadaInput
{
    public int ColetaId { get; set; }
    public string AmostraId { get; set; } = "";
    public string Parametro { get; set; } = "";
    public double ValorEsperado { get; set; }
    public string Unidade { get; set; } = "";
    public string Origem { get; set; } = "";
    public bool Sintetico { get; set; }
}

[ApiController]
[Authorize]
[Route("api/comparacoes-esperadas")]
public sealed class ComparacoesEsperadasController(WaterPathDbContext context,
    ResourceAuthorizationService authorization) : ControllerBase
{
    private static readonly IReadOnlyDictionary<string, string> Parametros = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Fe"] = "mg/L", ["Mn"] = "mg/L", ["Cr"] = "µg/L", ["Ni"] = "µg/L",
        ["Cu"] = "µg/L", ["Zn"] = "µg/L", ["Cd"] = "µg/L", ["Pb"] = "µg/L",
    };

    private int UsuarioId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
        ? id : 0;

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ComparacaoEsperadaInput input,
        CancellationToken cancellationToken)
    {
        var amostraId = input.AmostraId?.Trim() ?? "";
        if (UsuarioId() == 0) return Unauthorized();
        if (input.ColetaId <= 0 || string.IsNullOrWhiteSpace(amostraId) ||
            amostraId.Length > 160 || !Parametros.TryGetValue(input.Parametro, out var unidadeEsperada) ||
            input.Unidade != unidadeEsperada || string.IsNullOrWhiteSpace(input.Origem) ||
            input.Origem.Length > 300 || !double.IsFinite(input.ValorEsperado) || input.ValorEsperado < 0)
            return BadRequest("Informe coleta, amostra, parâmetro de saída, valor finito, unidade e origem válidos.");
        if (!await authorization.PodeAnalisarColetaAsync(input.ColetaId, UsuarioId(), cancellationToken))
            return Forbid();

        var existing = await context.ComparacoesEsperadas.SingleOrDefaultAsync(x =>
            x.AmostraId == amostraId && x.Parametro == input.Parametro, cancellationToken);
        if (existing is not null)
        {
            var identical = existing.ColetaId == input.ColetaId &&
                existing.ValorEsperado.Equals(input.ValorEsperado) && existing.Unidade == input.Unidade &&
                existing.Origem == input.Origem && existing.Sintetico == input.Sintetico;
            return identical ? Ok(existing) : Conflict("A chave amostra/parâmetro já possui outro valor esperado.");
        }

        var entity = new ComparacaoEsperadaEntity
        {
            ColetaId = input.ColetaId,
            AmostraId = amostraId,
            Parametro = input.Parametro,
            ValorEsperado = input.ValorEsperado,
            ValorPrevisto = null,
            Unidade = input.Unidade,
            Origem = input.Origem.Trim(),
            Sintetico = input.Sintetico,
            CriadoEm = DateTimeOffset.UtcNow,
        };
        context.ComparacoesEsperadas.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Listar), new { coletaId = entity.ColetaId }, entity);
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int coletaId, CancellationToken cancellationToken)
    {
        if (UsuarioId() == 0) return Unauthorized();
        if (coletaId <= 0) return BadRequest("Informe um coletaId válido.");
        if (!await authorization.PodeAnalisarColetaAsync(coletaId, UsuarioId(), cancellationToken))
            return Forbid();
        var values = await context.ComparacoesEsperadas.AsNoTracking()
            .Where(x => x.ColetaId == coletaId).OrderBy(x => x.AmostraId).ThenBy(x => x.Parametro)
            .ToListAsync(cancellationToken);
        return Ok(values);
    }
}
