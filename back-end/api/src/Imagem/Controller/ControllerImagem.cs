using System;
using Application.Commands.Imagem;
using Application.Queries.Coleta;
using Application.Queries.CorpoHidrico;
using Application.Queries.Imagem;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.Imagem;
using Domain.User;
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
        public async Task<IActionResult> Cadastrar([FromBody] CadastrarImagemDTO imagem)
        {
            try
            {
                var dataUpload = DateTime.UtcNow;
                var corpoHidrico = await mediator.Send(new QueryObterCorpoHidricoPorId { Id = imagem.IdCorpoHidrico });
                if (corpoHidrico == null)
                    return NotFound("Corpo Hídrico não encontrado");
                ColetaEntity? coleta = null;
                if (imagem.idColeta.HasValue)                {
                    coleta = await mediator.Send(new QueryObterColetaPorId { Id = imagem.idColeta.Value });
                    if (coleta == null)
                        return NotFound("Coleta não encontrada");
                }
                var extension = Path.GetExtension(imagem.Imagem.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";

                var path = Path.Combine(
                    "src",
                    "resources",
                    corpoHidrico.Id.ToString(),
                    dataUpload.ToString("yyyyMMdd")
                );

                Directory.CreateDirectory(path);

                var filePath = Path.Combine(path, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagem.Imagem.CopyToAsync(stream);
                }

                var imagemEntity = new ImagemEntity
                {
                    Url = filePath.Replace("\\", "/"),
                    DataUpload = dataUpload,
                    CorpoHidrico = corpoHidrico,
                    Coleta = coleta,
                };

                await mediator.Send(new CommandCadastrarImagem { Imagem = imagemEntity });
                return Ok("Imagem cadastrada com sucesso");
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
