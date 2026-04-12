using System;
using System.Collections.Generic;
using back_end.src.Domain.Imagem;
using back_end.src.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.Imagem
{
    [ApiController]
    [Route("api/imagem")]
    public class ControllerImagem : ControllerBase
    {
        private readonly IImagemRepository repository;

        public ControllerImagem(IImagemRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost]
        public IActionResult Cadastrar([FromBody] ImagemEntity imagem)
        {
            try
            {
                repository.Cadastrar(imagem);
                return Created($"api/imagem/{imagem.Id}", imagem);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult ObterPorId(int id)
        {
            try
            {
                var imagem = repository.ObterPorId(id);
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
        public IActionResult ObterTodos()
        {
            try
            {
                var imagens = repository.ObterTodos();
                return Ok(imagens);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, [FromBody] ImagemEntity imagem)
        {
            try
            {
                repository.Atualizar(imagem);
                return Ok("Imagem atualizada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Deletar(int id)
        {
            try
            {
                repository.Deletar(id);
                return Ok("Imagem deletada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
