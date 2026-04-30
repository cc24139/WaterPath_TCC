using System;
using Application.Commands.Imagem;
using Application.Queries.Imagem;
using back_end.src.Domain.Imagem;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.Imagem
{
    [ApiController]
    [Route("api/imagem")]
    public class ControllerImagem : ControllerBase
    {
        private readonly IMediator mediator;

        public ControllerImagem(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] ImagemEntity imagem)
        {
            try
            {
                await mediator.Send(new CommandCadastrarImagem { Imagem = imagem });
                return Created($"api/imagem/{imagem.Id}", imagem);
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
                var imagem = await mediator.Send(new QueryObterImagemPorId { Id = id });
                if (imagem == null)
                    return NotFound("Imagem não encontrada");
                return Ok(imagem);
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
                var imagens = await mediator.Send(new QueryObterTodasImagens());
                return Ok(imagens);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] ImagemEntity imagem)
        {
            try
            {
                await mediator.Send(new CommandAtualizarImagem { Imagem = imagem });
                return Ok("Imagem atualizada com sucesso");
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
                await mediator.Send(new CommandDeletarImagem { Id = id });
                return Ok("Imagem deletada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
