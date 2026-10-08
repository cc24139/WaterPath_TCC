using System;
using Application.Commands.Coleta;
using Application.DTOs.Coleta;
using Application.Queries.Coleta;
using back_end.src.Domain.Coleta;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace back_end.src.Controllers.Coleta
{
    [ApiController]
    [Route("api/coleta")]
    public class ControllerColeta : ControllerBase
    {
        private readonly IMediator mediator;

        public ControllerColeta(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] ColetaCadastroInput input)
        {
            try
            {
                if (Request.Headers.ContainsKey("Authorization") && User.Identity?.IsAuthenticated != true)
                    return Unauthorized("Sessão inválida. Entre novamente para cadastrar a coleta.");
                var coleta = input.ToEntity();
                if (User.Identity?.IsAuthenticated == true)
                {
                    if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var responsavelId)
                        || string.IsNullOrWhiteSpace(User.Identity.Name))
                        return Unauthorized("Identificação do responsável indisponível. Entre novamente.");
                    coleta.RegistrarResponsavel(responsavelId, User.Identity.Name);
                }
                await mediator.Send(
                    new CommandCadastrarColeta
                    {
                        Coleta = coleta,
                        CorpoHidricoId = coleta.CorpoHidricoId,
                    }
                );
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

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            try
            {
                var coleta = await mediator.Send(new QueryObterColetaPorId { Id = id });
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
        public async Task<IActionResult> ObterTodos()
        {
            try
            {
                var coletas = await mediator.Send(new QueryObterTodasColetas());
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
        public async Task<IActionResult> ObterPorCorpoHidrico(int idCorpoHidrico)
        {
            try
            {
                var coletas = await mediator.Send(new QueryObterColetasPorCorpoHidrico { CorpoHidricoId = idCorpoHidrico });
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
        public async Task<IActionResult> ObterPorPeriodo(int corpoHidricoId, [FromQuery] DateTime dataInicio, [FromQuery] DateTime dataFim)
        {
            try
            {
                var coletas = await mediator.Send(new QueryObterColetasPorPeriodo { CorpoHidricoId = corpoHidricoId, DataInicio = dataInicio, DataFim = dataFim });
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
        public async Task<IActionResult> ObterPorData(DateTime data)
        {
            try
            {
                var idCorpoHidrico = User.Claims.FirstOrDefault(c => c.Type == "corpoHidricoId")?.Value;
                var coletas = await mediator.Send(new QueryObterColetasPorData { Data = data });
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] ColetaInput input)
        {
            try
            {
                await mediator.Send(new CommandAtualizarColeta { Coleta = input.ToEntity(), ColetaId = id });
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            try
            {
                await mediator.Send(new CommandDeletarColeta { Id = id });
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
