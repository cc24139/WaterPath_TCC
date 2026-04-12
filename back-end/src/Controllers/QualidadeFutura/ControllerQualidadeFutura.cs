using System;
using System.Collections.Generic;
using back_end.src.Domain.QualidadeFutura;
using back_end.src.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.QualidadeFutura
{
    [ApiController]
    [Route("api/qualidadefutura")]
    public class ControllerQualidadeFutura : ControllerBase
    {
        private readonly IQualidadeFuturaRepository repository;

        public ControllerQualidadeFutura(IQualidadeFuturaRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost]
        public IActionResult Cadastrar([FromBody] QualidadeFuturaEntity qualidadeFutura)
        {
            try
            {
                repository.Cadastrar(qualidadeFutura);
                return Created($"api/qualidadefutura/{qualidadeFutura.Id}", qualidadeFutura);
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
                var qualidadeFutura = repository.ObterPorId(id);
                if (qualidadeFutura == null)
                    return NotFound("Qualidade futura não encontrada");
                return Ok(qualidadeFutura);
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
                var qualidadesFuturas = repository.ObterTodos();
                return Ok(qualidadesFuturas);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, [FromBody] QualidadeFuturaEntity qualidadeFutura)
        {
            try
            {
                repository.Atualizar(qualidadeFutura);
                return Ok("Qualidade futura atualizada com sucesso");
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
                return Ok("Qualidade futura deletada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
