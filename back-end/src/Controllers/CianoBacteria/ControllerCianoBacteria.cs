using System;
using System.Collections.Generic;
using back_end.src.Domain.CianoBacteria;
using back_end.src.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.CianoBacteria
{
    [ApiController]
    [Route("api/cianobacteria")]
    public class ControllerCianoBacteria : ControllerBase
    {
        private readonly ICianoBacteriaRepository repository;

        public ControllerCianoBacteria(ICianoBacteriaRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost]
        public IActionResult Cadastrar([FromBody] CianoBacteriaEntity cianoBacteria)
        {
            try
            {
                repository.Cadastrar(cianoBacteria);
                return Created($"api/cianobacteria/{cianoBacteria.Id}", cianoBacteria);
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
                var cianoBacteria = repository.ObterPorId(id);
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
        public IActionResult ObterTodos()
        {
            try
            {
                var cianoBacterias = repository.ObterTodos();
                return Ok(cianoBacterias);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, [FromBody] CianoBacteriaEntity cianoBacteria)
        {
            try
            {
                repository.Atualizar(cianoBacteria);
                return Ok("Cianobactéria atualizada com sucesso");
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
                return Ok("Cianobactéria deletada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
