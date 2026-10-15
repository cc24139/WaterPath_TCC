using System;
using Application.Commands.Qualidade;
using Application.Queries.Qualidade;
using back_end.src.Domain.Qualidade;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.Qualidade
{
    [ApiController]
    [Route("api/qualidade")]
    public class ControllerQualidade : ControllerBase
    {
        private readonly IMediator mediator;

        public ControllerQualidade(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] QualidadeEntity qualidade, CancellationToken cancellationToken)
        {
            try
            {
                await mediator.Send(new CommandCadastrarQualidade { Qualidade = qualidade }, cancellationToken);
                return Created($"api/qualidade/{qualidade.Id}", qualidade);
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
                var qualidade = await mediator.Send(new QueryObterQualidadePorId { Id = id }, cancellationToken);
                if (qualidade == null)
                    return NotFound("Qualidade não encontrada");
                return Ok(qualidade);
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
                var qualidades = await mediator.Send(new QueryObterTodasQualidades(), cancellationToken);
                return Ok(qualidades.Select(q => new
                {
                    q.Id,
                    q.IQA,
                    CorpoHidrico = q.CorpoHidrico == null ? null : new { q.CorpoHidrico.Id },
                    QualidadeFutura = q.QualidadeFutura == null ? null : new { q.QualidadeFutura.Id },
                }));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] QualidadeEntity qualidade, CancellationToken cancellationToken)
        {
            try
            {
                await mediator.Send(new CommandAtualizarQualidade { Qualidade = qualidade }, cancellationToken);
                return Ok("Qualidade atualizada com sucesso");
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
                await mediator.Send(new CommandDeletarQualidade { Id = id }, cancellationToken);
                return Ok("Qualidade deletada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
