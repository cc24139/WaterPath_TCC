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
        public async Task<IActionResult> Cadastrar([FromBody] QualidadeFuturaEntity qualidadeFutura)
        {
            try
            {
                await mediator.Send(
                    new CommandCadastrarQualidadeFutura { QualidadeFutura = qualidadeFutura }
                );
                return Created($"api/qualidadefutura/{qualidadeFutura.Id}", qualidadeFutura);
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
                var qualidadeFutura = await mediator.Send(
                    new QueryObterQualidadeFuturaPorId { Id = id }
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
        public async Task<IActionResult> ObterTodos()
        {
            try
            {
                var qualidadesFuturas = await mediator.Send(new QueryObterTodasQualidadesFuturas());
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
            [FromBody] QualidadeFuturaEntity qualidadeFutura
        )
        {
            try
            {
                await mediator.Send(
                    new CommandAtualizarQualidadeFutura { QualidadeFutura = qualidadeFutura }
                );
                return Ok("Qualidade futura atualizada com sucesso");
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
                await mediator.Send(new CommandDeletarQualidadeFutura { Id = id });
                return Ok("Qualidade futura deletada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
