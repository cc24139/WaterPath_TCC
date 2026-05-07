using System;
using Application.Commands.Coleta;
using Application.Queries.Coleta;
using back_end.src.Domain.Coleta;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Cadastrar([FromBody] ColetaEntity coleta)
        {
            try
            {
                await mediator.Send(
                    new CommandCadastrarColeta
                    {
                        Coleta = coleta,
                        CorpoHidricoId = coleta.CorpoHidrico.Id,
                    }
                );
                return Created($"api/coleta/{coleta.Id}", coleta);
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
                return Ok(coletas);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] ColetaEntity coleta)
        {
            try
            {
                await mediator.Send(new CommandAtualizarColeta { Coleta = coleta, ColetaId = id });
                return Ok("Coleta atualizada com sucesso");
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
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
