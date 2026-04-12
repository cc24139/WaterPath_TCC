using System;
using System.Collections.Generic;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.CorpoHidrico
{
    [ApiController]
    [Route("api/corpohidrico")]
    public class ControllerCorpoHidrico : ControllerBase
    {
        private readonly ICorpoHidricoRepository repository;

        public ControllerCorpoHidrico(ICorpoHidricoRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost]
        public IActionResult Cadastrar([FromBody] CorpoHidricoEntity corpoHidrico)
        {
            try
            {
                repository.Cadastrar(corpoHidrico);
                return Created($"api/corpohidrico/{corpoHidrico.Id}", corpoHidrico);
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
                var corpoHidrico = repository.ObterCorpoHidricoPorId(id);
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
        public IActionResult ObterTodos()
        {
            try
            {
                var corposHidricos = repository.ObterTodos();
                return Ok(corposHidricos);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, [FromBody] CorpoHidricoEntity corpoHidrico)
        {
            try
            {
                repository.Atualizar(corpoHidrico);
                return Ok("Corpo hídrico atualizado com sucesso");
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
                return Ok("Corpo hídrico deletado com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
