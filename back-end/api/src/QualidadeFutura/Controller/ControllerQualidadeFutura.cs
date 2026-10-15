using System;
using Application.Commands.QualidadeFutura;
using Application.Queries.QualidadeFutura;
using back_end.src.Domain.QualidadeFutura;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.QualidadeFutura
{
    [ApiController]
    [Route("api/qualidadefutura")]
    public class ControllerQualidadeFutura : ControllerBase
    {
        private readonly IMediator mediator;

        public ControllerQualidadeFutura(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] QualidadeFuturaEntity qualidadeFutura, CancellationToken cancellationToken)
        {
            try
            {
                await mediator.Send(
                    new CommandCadastrarQualidadeFutura { QualidadeFutura = qualidadeFutura },
                    cancellationToken
                );
                return Created($"api/qualidadefutura/{qualidadeFutura.Id}", qualidadeFutura);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        {
            try
            {
                var qualidadeFutura = await mediator.Send(
                    new QueryObterQualidadeFuturaPorId { Id = id },
                    cancellationToken
                );
                if (qualidadeFutura == null)
                    return NotFound("Qualidade futura não encontrada");
                return Ok(qualidadeFutura);
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
                var qualidadesFuturas = await mediator.Send(new QueryObterTodasQualidadesFuturas(), cancellationToken);
                return Ok(qualidadesFuturas);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(
            int id,
            [FromBody] QualidadeFuturaEntity qualidadeFutura,
            CancellationToken cancellationToken
        )
        {
            try
            {
                await mediator.Send(
                    new CommandAtualizarQualidadeFutura { QualidadeFutura = qualidadeFutura },
                    cancellationToken
                );
                return Ok("Qualidade futura atualizada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id, CancellationToken cancellationToken)
        {
            try
            {
                await mediator.Send(new CommandDeletarQualidadeFutura { Id = id }, cancellationToken);
                return Ok("Qualidade futura deletada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
