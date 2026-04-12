using System;
using System.Collections.Generic;
using back_end.src.Domain.Qualidade;
using back_end.src.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.Qualidade
{
    [ApiController]
    [Route("api/qualidade")]
    public class ControllerQualidade : ControllerBase
    {
        private readonly IQualidadeRepository repository;

        public ControllerQualidade(IQualidadeRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost]
        public IActionResult Cadastrar([FromBody] QualidadeEntity qualidade)
        {
            try
            {
                repository.Cadastrar(qualidade);
                return Created($"api/qualidade/{qualidade.Id}", qualidade);
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
                var qualidade = repository.ObterPorId(id);
                if (qualidade == null)
                    return NotFound("Qualidade não encontrada");
                return Ok(qualidade);
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
                var qualidades = repository.ObterTodos();
                return Ok(qualidades);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, [FromBody] QualidadeEntity qualidade)
        {
            try
            {
                repository.Atualizar(qualidade);
                return Ok("Qualidade atualizada com sucesso");
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
                return Ok("Qualidade deletada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
