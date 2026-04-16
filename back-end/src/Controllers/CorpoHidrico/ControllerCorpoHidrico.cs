using System;
using Application.Commands.CorpoHidrico;
using Application.Queries.CorpoHidrico;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.CorpoHidrico
{
    [ApiController]
    [Route("api/corpohidrico")]
    public class ControllerCorpoHidrico : ControllerBase
    {
        private readonly IMediator mediator;

        public ControllerCorpoHidrico(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] CommandCadastrarCorpoHidrico command)
        {
            try
            {
                await mediator.Send(command);
                return Ok("Corpo hídrico cadastrado com sucesso");
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
                var corpoHidrico = await mediator.Send(new QueryObterCorpoHidricoPorId { Id = id });
                if (corpoHidrico == null)
                    return NotFound("Corpo hídrico não encontrado");
                return Ok(corpoHidrico);
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
                var corposHidricos = await mediator.Send(new QueryObterTodosCorposHidricos());
                return Ok(corposHidricos);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(
            int id,
            [FromBody] CommandAtualizarCorpoHidrico corpoHidrico
        )
        {
            try
            {
                corpoHidrico.Id = id;
                await mediator.Send(corpoHidrico);
                return Ok("Corpo hídrico atualizado com sucesso");
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
                await mediator.Send(new CommandDeletarCorpoHidrico { Id = id });
                return Ok("Corpo hídrico deletado com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
