using System;
using System.Collections.Generic;
using back_end.src.Domain.Coleta;
using back_end.src.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.Coleta
{
    [ApiController]
    [Route("api/coleta")]
    public class ControllerColeta : ControllerBase
    {
        private readonly IColetaRepository repository;

        public ControllerColeta(IColetaRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost]
        public IActionResult Cadastrar([FromBody] ColetaEntity coleta)
        {
            try
            {
                repository.Cadastrar(coleta);
                return Created($"api/coleta/{coleta.Id}", coleta);
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
                var coleta = repository.ObterPorId(id);
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
        public IActionResult ObterTodos()
        {
            try
            {
                var coletas = repository.ObterTodos();
                return Ok(coletas);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, [FromBody] ColetaEntity coleta)
        {
            try
            {
                repository.Atualizar(coleta);
                return Ok("Coleta atualizada com sucesso");
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
                return Ok("Coleta deletada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
