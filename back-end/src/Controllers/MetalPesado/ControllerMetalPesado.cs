using System;
using System.Collections.Generic;
using back_end.src.Domain.MetalPesado;
using back_end.src.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.MetalPesado
{
    [ApiController]
    [Route("api/metalpesado")]
    public class ControllerMetalPesado : ControllerBase
    {
        private readonly IMetalPesadoRepository repository;

        public ControllerMetalPesado(IMetalPesadoRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost]
        public IActionResult Cadastrar([FromBody] MetalPesadoEntity metalPesado)
        {
            try
            {
                repository.Cadastrar(metalPesado);
                return Created($"api/metalpesado/{metalPesado.Id}", metalPesado);
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
                var metalPesado = repository.ObterPorId(id);
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
        public IActionResult ObterTodos()
        {
            try
            {
                var metaisPesados = repository.ObterTodos();
                return Ok(metaisPesados);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, [FromBody] MetalPesadoEntity metalPesado)
        {
            try
            {
                repository.Atualizar(metalPesado);
                return Ok("Metal pesado atualizado com sucesso");
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
                return Ok("Metal pesado deletado com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
