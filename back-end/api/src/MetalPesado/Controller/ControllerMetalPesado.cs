using System;
using Application.Commands.MetalPesado;
using Application.Queries.MetalPesado;
using back_end.src.Domain.MetalPesado;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.MetalPesado
{
    [ApiController]
    [Route("api/metalpesado")]
    public class ControllerMetalPesado : ControllerBase
    {
        private readonly IMediator mediator;

        public ControllerMetalPesado(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] MetalPesadoEntity metalPesado, CancellationToken cancellationToken)
        {
            try
            {
                await mediator.Send(new CommandCadastrarMetalPesado { MetalPesado = metalPesado }, cancellationToken);
                return Created($"api/metalpesado/{metalPesado.Id}", metalPesado);
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
                var metalPesado = await mediator.Send(new QueryObterMetalPesadoPorId { Id = id }, cancellationToken);
                if (metalPesado == null)
                    return NotFound("Metal pesado não encontrado");
                return Ok(metalPesado);
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
                var metaisPesados = await mediator.Send(new QueryObterTodosMetaisPesados(), cancellationToken);
                return Ok(metaisPesados);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] MetalPesadoEntity metalPesado, CancellationToken cancellationToken)
        {
            try
            {
                await mediator.Send(new CommandAtualizarMetalPesado { MetalPesado = metalPesado }, cancellationToken);
                return Ok("Metal pesado atualizado com sucesso");
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
                await mediator.Send(new CommandDeletarMetalPesado { Id = id }, cancellationToken);
                return Ok("Metal pesado deletado com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
