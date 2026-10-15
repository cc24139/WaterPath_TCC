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
        public async Task<IActionResult> Cadastrar([FromBody] CianoBacteriaEntity cianoBacteria, CancellationToken cancellationToken)
        {
            try
            {
                await mediator.Send(
                    new CommandCadastrarCianoBacteria { CianoBacteria = cianoBacteria },
                    cancellationToken
                );
                return Created($"api/cianobacteria/{cianoBacteria.Id}", cianoBacteria);
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
                var cianoBacteria = await mediator.Send(
                    new QueryObterCianoBacteriaPorId { Id = id },
                    cancellationToken
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
        public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
        {
            try
            {
                var cianoBacterias = await mediator.Send(new QueryObterTodasCianoBacterias(), cancellationToken);
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
            [FromBody] CianoBacteriaEntity cianoBacteria,
            CancellationToken cancellationToken
        )
        {
            try
            {
                await mediator.Send(
                    new CommandAtualizarCianoBacteria { CianoBacteria = cianoBacteria },
                    cancellationToken
                );
                return Ok("Cianobactéria atualizada com sucesso");
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
                await mediator.Send(new CommandDeletarCianoBacteria { Id = id }, cancellationToken);
                return Ok("Cianobactéria deletada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
