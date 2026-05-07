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
        public async Task<IActionResult> Cadastrar([FromBody] QualidadeEntity qualidade)
        {
            try
            {
                await mediator.Send(new CommandCadastrarQualidade { Qualidade = qualidade });
                return Created($"api/qualidade/{qualidade.Id}", qualidade);
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
                var qualidade = await mediator.Send(new QueryObterQualidadePorId { Id = id });
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
        public async Task<IActionResult> ObterTodos()
        {
            try
            {
                var qualidades = await mediator.Send(new QueryObterTodasQualidades());
                return Ok(qualidades);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] QualidadeEntity qualidade)
        {
            try
            {
                await mediator.Send(new CommandAtualizarQualidade { Qualidade = qualidade });
                return Ok("Qualidade atualizada com sucesso");
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
                await mediator.Send(new CommandDeletarQualidade { Id = id });
                return Ok("Qualidade deletada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
