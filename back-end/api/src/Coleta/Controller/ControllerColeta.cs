using System;
using Application.Commands.Coleta;
using Application.DTOs.Coleta;
using Application.Queries.Coleta;
using back_end.src.Domain.Coleta;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using back_end.src.Authorization;
using back_end.src.Requests;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.Controllers.Coleta
{
    [ApiController]
    [Route("api/coleta")]
    public class ControllerColeta : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ResourceAuthorizationService authorization;
        private readonly WaterPathDbContext context;

        public ControllerColeta(IMediator mediator, ResourceAuthorizationService authorization, WaterPathDbContext context)
        {
            this.mediator = mediator;
            this.authorization = authorization;
            this.context = context;
        }

        private int UsuarioId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] ColetaCadastroInput input, CancellationToken cancellationToken,
            [FromHeader(Name = "Idempotency-Key")] string? requestKey = null)
        {
            try
            {
                if (UsuarioId() <= 0 || string.IsNullOrWhiteSpace(User.Identity?.Name))
                    return Unauthorized("Identificação do responsável indisponível. Entre novamente.");
                if (input.CorpoHidricoId <= 0) return BadRequest("Id inválido do corpo hídrico.");
                if (!await authorization.PodeCadastrarColetaAsync(input.CorpoHidricoId, UsuarioId(), cancellationToken)) return Forbid();
                if (!RequestIdempotency.TryParse(requestKey, out var key)) return BadRequest("Idempotency-Key deve ser um UUID válido.");
                var hash = key.HasValue ? RequestIdempotency.HashJson(input) : null;
                if (key.HasValue)
                {
                    var prior = await BuscarPorChave(key.Value, cancellationToken);
                    if (prior is not null) return RespostaRepetida(prior, hash!);
                }
                var coleta = input.ToEntity();
                coleta.RegistrarResponsavel(UsuarioId(), User.Identity.Name!);
                if (key.HasValue) coleta.RegistrarRequisicao(key.Value, hash!);
                try
                {
                    await mediator.Send(new CommandCadastrarColeta
                    {
                        Coleta = coleta, CorpoHidricoId = coleta.CorpoHidricoId,
                    }, cancellationToken);
                }
                catch (DbUpdateException) when (key.HasValue)
                {
                    // Another request may have committed the same key while this one was saving.
                    var prior = await BuscarPorChave(key.Value, cancellationToken);
                    if (prior is null) throw;
                    return RespostaRepetida(prior, hash!);
                }
                return Created($"api/coleta/{coleta.Id}", coleta);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private Task<ColetaEntity?> BuscarPorChave(Guid key, CancellationToken cancellationToken) =>
            context.Coletas.AsNoTracking().Include(c => c.Medicoes).Include(c => c.MetaisPesados)
                .AsSplitQuery().SingleOrDefaultAsync(c => c.CadastroRequestKey == key, cancellationToken);

        private IActionResult RespostaRepetida(ColetaEntity prior, string hash) =>
            prior.ResponsavelId == UsuarioId() && prior.CadastroRequestHash == hash
                ? Created($"api/coleta/{prior.Id}", prior)
                : Conflict("Esta chave de requisição já foi usada com outros dados.");

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        {
            try
            {
                var coleta = await mediator.Send(new QueryObterColetaPorId { Id = id }, cancellationToken);
                if (coleta == null)
                    return NotFound("Coleta não encontrada");
                return Ok(coleta);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
        {
            try
            {
                var coletas = await mediator.Send(new QueryObterTodasColetas(), cancellationToken);
                // Project the relation to its ID: entity graphs can contain cycles.
                return Ok(coletas.Select(c => new
                {
                    c.Id,
                    c.DataHora,
                    c.Medicoes,
                    c.MetaisPesados,
                    c.ResponsavelId,
                    c.ResponsavelNome,
                    c.CorpoHidricoId,
                }));    
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("corpo-hidrico/{idCorpoHidrico}")]
        public async Task<IActionResult> ObterPorCorpoHidrico(int idCorpoHidrico, CancellationToken cancellationToken)
        {
            try
            {
                var coletas = await mediator.Send(new QueryObterColetasPorCorpoHidrico { CorpoHidricoId = idCorpoHidrico }, cancellationToken);
                return Ok(coletas);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("periodo/{corpoHidricoId}")]
        public async Task<IActionResult> ObterPorPeriodo(int corpoHidricoId, [FromQuery] DateTime dataInicio, [FromQuery] DateTime dataFim, CancellationToken cancellationToken)
        {
            try
            {
                var coletas = await mediator.Send(new QueryObterColetasPorPeriodo { CorpoHidricoId = corpoHidricoId, DataInicio = dataInicio, DataFim = dataFim }, cancellationToken);
                return Ok(coletas);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("data/{data}")]
        public async Task<IActionResult> ObterPorData(DateTime data, CancellationToken cancellationToken)
        {
            try
            {
                var idCorpoHidrico = User.Claims.FirstOrDefault(c => c.Type == "corpoHidricoId")?.Value;
                var coletas = await mediator.Send(new QueryObterColetasPorData { Data = data }, cancellationToken);
                return Ok(coletas);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] ColetaInput input, CancellationToken cancellationToken)
        {
            try
            {
                if (!await authorization.PodeGerenciarColetaAsync(id, UsuarioId(), cancellationToken)
                    || !await authorization.PodeGerenciarCorpoHidricoAsync(input.CorpoHidricoId, UsuarioId(), cancellationToken)) return Forbid();
                await mediator.Send(new CommandAtualizarColeta { Coleta = input.ToEntity(), ColetaId = id }, cancellationToken);
                return Ok("Coleta atualizada com sucesso");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id, CancellationToken cancellationToken)
        {
            try
            {
                if (!await authorization.PodeGerenciarColetaAsync(id, UsuarioId(), cancellationToken)) return Forbid();
                await mediator.Send(new CommandDeletarColeta { Id = id }, cancellationToken);
                return Ok("Coleta deletada com sucesso");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
