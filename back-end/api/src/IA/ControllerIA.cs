using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using back_end.src.IA.Application;
using back_end.src.Authorization;
using back_end.src.Requests;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.IA;

public class PredicaoIAInput
{
    [Range(1, int.MaxValue)] public int ColetaId { get; set; }
    [Required] public IFormFile Image { get; set; } = null!;
    [FromForm(Name = "amostra")] public string? Amostra { get; set; }
    // Compatibilidade temporária com clientes anteriores. Novos clientes usam "amostra".
    [FromForm(Name = "data")] public string? Data { get; set; }

    public string ObterAmostra()
    {
        if (!string.IsNullOrWhiteSpace(Amostra) && !string.IsNullOrWhiteSpace(Data)
            && !string.Equals(Amostra, Data, StringComparison.Ordinal))
            throw new IaException(400, IaErrorCodes.InvalidInput,
                "Envie a amostra uma única vez.");
        return !string.IsNullOrWhiteSpace(Amostra) ? Amostra
            : !string.IsNullOrWhiteSpace(Data) ? Data
            : throw new IaException(400, IaErrorCodes.InvalidInput,
                "Informe os dados da amostra.");
    }
}

public class AnaliseLagoInput
{
    [Required] public IFormFile Image { get; set; } = null!;
}

[ApiController]
[Authorize]
[IaExceptionFilter]
[Route("api/ia/predicoes")]
public class ControllerIA(WaterPathDbContext context, PredicaoIAService service,
    ResourceAuthorizationService authorization) : ControllerBase

{
    private int UsuarioId()
    {
        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0) return id;
        throw new IaException(401, IaErrorCodes.Unauthorized,
            "A sessão não contém uma identificação de usuário válida.");
    }

    private async Task GarantirAcessoColetaAsync(int coletaId, CancellationToken cancellationToken)
    {
        if (!await authorization.PodeAnalisarColetaAsync(coletaId, UsuarioId(), cancellationToken))
            throw new IaException(403, IaErrorCodes.Forbidden,
                "Você não tem permissão para acessar esta análise.");
    }

    private async Task GarantirAcessoCorpoHidricoAsync(int corpoHidricoId, CancellationToken cancellationToken)
    {
        if (!await authorization.PodeAnalisarCorpoHidricoAsync(corpoHidricoId, UsuarioId(), cancellationToken))
            throw new IaException(403, IaErrorCodes.Forbidden,
                "Você não tem permissão para analisar este corpo hídrico.");
    }


    [HttpPost("corpo-hidrico/{corpoHidricoId:int}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(PredicaoIAService.LimiteImagem + 1024 * 1024)]
    public async Task<IActionResult> AnalisarLago(int corpoHidricoId, [FromForm] AnaliseLagoInput input,
        CancellationToken cancellationToken)
    {
        if (!await authorization.PodeIniciarAnaliseCorpoHidricoAsync(corpoHidricoId, UsuarioId(), cancellationToken))
            throw new IaException(403, IaErrorCodes.Forbidden,
                "Você não tem permissão para analisar este corpo hídrico.");
        var result = await service.AnalisarLagoAsync(corpoHidricoId, input.Image, cancellationToken);
        return RespostaCriada(result);
    }

    private IActionResult RespostaCriada(back_end.src.IA.Domain.PredicaoIAEntity result) =>
        CreatedAtAction(nameof(Obter), new { id = result.Id }, PredicaoIAPublicContract.Criar(result));

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(PredicaoIAService.LimiteImagem + 1024 * 1024)]
    public async Task<IActionResult> Predizer([FromForm] PredicaoIAInput input, CancellationToken cancellationToken,
        [FromHeader(Name = "Idempotency-Key")] string? requestKey = null)
    {
        await GarantirAcessoColetaAsync(input.ColetaId, cancellationToken);
        if (!RequestIdempotency.TryParse(requestKey, out var key)) return BadRequest("Idempotency-Key deve ser um UUID válido.");
        var sample = input.ObterAmostra();
        var hash = key.HasValue ? await RequestIdempotency.HashAnalysisAsync(input.Image, sample, cancellationToken) : null;
        if (key.HasValue)
        {
            var prior = await context.PredicoesIA.AsNoTracking()
                .SingleOrDefaultAsync(p => p.AnaliseRequestKey == key.Value, cancellationToken);
            if (prior is not null)
                return prior.ColetaId == input.ColetaId && prior.AnaliseRequestHash == hash
                    ? RespostaCriada(prior)
                    : Conflict("Esta chave de requisição já foi usada com outros dados.");
        }
        var result = await service.CriarAsync(input.ColetaId, input.Image, sample, cancellationToken, key, hash);
        return RespostaCriada(result);
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int? coletaId, CancellationToken cancellationToken,
        [FromQuery, Range(1, int.MaxValue)] int pagina = 1, [FromQuery, Range(1, 100)] int tamanhoPagina = 20)
    {
        var query = authorization.FiltrarPredicoesDoUsuario(context.PredicoesIA.AsNoTracking(), UsuarioId());
        if (coletaId.HasValue) query = query.Where(p => p.ColetaId == coletaId.Value);
        var total = await query.CountAsync(cancellationToken);
        var itens = await query.OrderByDescending(p => p.CriadaEm).ThenByDescending(p => p.Id)
            .Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina)
            .Select(p => new { p.Id, p.ColetaId, p.CorpoHidricoId, p.DataColeta, p.Tipo, p.CriadaEm }).ToListAsync(cancellationToken);
        return Ok(new { total, pagina, tamanhoPagina, itens });
    }

    [HttpGet("resumos")]
    public async Task<IActionResult> Resumos(CancellationToken cancellationToken,
        [FromQuery, Range(1, int.MaxValue)] int? corpoHidricoId = null,
        [FromQuery] bool somenteMinhas = false, [FromQuery] int[]? coletaIds = null,
        [FromQuery, StringLength(100)] string? busca = null, [FromQuery] bool antigasPrimeiro = false,
        [FromQuery, Range(1, 1000000)] int pagina = 1, [FromQuery, Range(1, 100)] int tamanhoPagina = 10)
    {
        if (coletaIds is { Length: > 100 } || coletaIds?.Any(id => id <= 0) == true)
            return BadRequest("Informe até 100 IDs de coleta válidos.");
        if (corpoHidricoId.HasValue) await GarantirAcessoCorpoHidricoAsync(corpoHidricoId.Value, cancellationToken);
        return Ok(await AnalysisSummaryQuery.ReadAsync(context, UsuarioId(), corpoHidricoId, somenteMinhas,
            coletaIds ?? [], busca, antigasPrimeiro, pagina, tamanhoPagina, cancellationToken));
    }

    [HttpGet("coletas/{coletaId:int}/retomada")]
    public async Task<IActionResult> RetomarColeta(int coletaId, CancellationToken cancellationToken)
    {
        var userId = UsuarioId();
        var coleta = await context.Coletas.AsNoTracking()
            .Where(c => c.Id == coletaId && c.ResponsavelId == userId)
            .Select(c => new
            {
                c.Id, c.CorpoHidricoId, c.DataHora,
                Medicoes = c.Medicoes.Select(m => new { m.codigoMedicao, m.valor, m.unidade, m.censurado }),
                MetaisPesados = c.MetaisPesados.Select(m => new { m.Nome, m.Concentracao, m.Unidade }),
            }).SingleOrDefaultAsync(cancellationToken);
        if (coleta is null) return NotFound("Coleta não encontrada para esta conta.");
        if (await context.PredicoesIA.AsNoTracking().AnyAsync(p => p.ColetaId == coletaId && p.Tipo == "integrada", cancellationToken))
            return Conflict("Esta coleta já possui análise integrada.");
        return Ok(coleta);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id, CancellationToken cancellationToken)
    {
        var p = await authorization.FiltrarPredicoesDoUsuario(context.PredicoesIA.AsNoTracking(), UsuarioId())
            .Where(p => p.Id == id)
            .Select(p => new { p.Id, p.ColetaId, p.CorpoHidricoId, p.DataColeta, p.Tipo, p.CriadaEm, p.ResultadoJson })
            .SingleOrDefaultAsync(cancellationToken);
        if (p is null) return NotFound("Predição não encontrada.");
        return Ok(PredicaoIAPublicContract.Criar(p.Id, p.ColetaId, p.CorpoHidricoId,
            p.DataColeta, p.Tipo, p.CriadaEm, p.ResultadoJson));
    }

    [HttpGet("{id:int}/imagem")]
    public async Task<IActionResult> Imagem(int id, CancellationToken cancellationToken)
    {
        var p = await authorization.FiltrarPredicoesDoUsuario(context.PredicoesIA.AsNoTracking(), UsuarioId())
            .Where(p => p.Id == id)
            .Select(p => new { p.ImagemResultado, p.ContentTypeResultado }).SingleOrDefaultAsync(cancellationToken);
        return p is null ? NotFound("Predição não encontrada.") : File(p.ImagemResultado, p.ContentTypeResultado, "predicao.jpg");
    }

    [HttpGet("{id:int}/imagem/original")]
    public async Task<IActionResult> ImagemOriginal(int id, CancellationToken cancellationToken)
    {
        var p = await authorization.FiltrarPredicoesDoUsuario(context.PredicoesIA.AsNoTracking(), UsuarioId())
            .Where(p => p.Id == id)
            .Select(p => new { p.ImagemOriginal, p.ContentTypeOriginal, p.NomeArquivo }).SingleOrDefaultAsync(cancellationToken);
        return p is null ? NotFound("Predição não encontrada.") : File(p.ImagemOriginal, p.ContentTypeOriginal, p.NomeArquivo);
    }

    [HttpGet("{id:int}/imagem/atual")]
    public async Task<IActionResult> ImagemAtual(int id, CancellationToken cancellationToken)
    {
        
            if (id <= 0)
                return new BadRequestObjectResult("Corpo hídrico inválido.");

            var imagem = await context.Imagens
            .Where(i => i.CorpoHidrico.Id == id)
            .OrderByDescending(i => i.DataUpload)
            .ThenByDescending(i => i.Id)
            .FirstOrDefaultAsync(cancellationToken);

            if (imagem == null)
                return new NotFoundObjectResult("Nenhuma imagem encontrada.");

            return new OkObjectResult(imagem);

    }
}
