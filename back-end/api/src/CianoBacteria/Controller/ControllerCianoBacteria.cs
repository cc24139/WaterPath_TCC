using Application.Commands.CianoBacteria;
using Application.Queries.CianoBacteria;
using back_end.src.Domain.CianoBacteria;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.CianoBacteria
{
    [ApiController]
    [Route("api/cianobacteria")]
    public class ControllerCianoBacteria : ControllerBase
    {
        private readonly IMediator mediator;

        public ControllerCianoBacteria(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] CianoBacteriaEntity cianoBacteria)
        {
            try
            {
                await mediator.Send(
                    new CommandCadastrarCianoBacteria { CianoBacteria = cianoBacteria }
                );
                return Created($"api/cianobacteria/{cianoBacteria.Id}", cianoBacteria);
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
                var cianoBacteria = await mediator.Send(
                    new QueryObterCianoBacteriaPorId { Id = id }
                );
                if (cianoBacteria == null)
                    return NotFound("Cianobactéria não encontrada");
                return Ok(cianoBacteria);
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
                var cianoBacterias = await mediator.Send(new QueryObterTodasCianoBacterias());
                return Ok(cianoBacterias);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(
            int id,
            [FromBody] CianoBacteriaEntity cianoBacteria
        )
        {
            try
            {
                await mediator.Send(
                    new CommandAtualizarCianoBacteria { CianoBacteria = cianoBacteria }
                );
                return Ok("Cianobactéria atualizada com sucesso");
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
                await mediator.Send(new CommandDeletarCianoBacteria { Id = id });
                return Ok("Cianobactéria deletada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
