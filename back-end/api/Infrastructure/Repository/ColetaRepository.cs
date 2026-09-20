using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.Coleta;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.Infrastructure.Repository
{
    public class ColetaRepository : IColetaRepository
    {
        private readonly WaterPathDbContext context;

        public ColetaRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public virtual void Atualizar(ColetaEntity coleta, int idColeta)
        {
            var entityToUpdate = context.Coletas.Include(c => c.Medicoes).SingleOrDefault(c => c.Id == idColeta);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Coleta não encontrada");
            }
            ValidarCorpoHidrico(coleta.CorpoHidricoId);
            entityToUpdate.CorpoHidricoId = coleta.CorpoHidricoId;
            entityToUpdate.DataHora = coleta.DataHora.ToUniversalTime();
            entityToUpdate.Latitude = coleta.Latitude;
            entityToUpdate.Longitude = coleta.Longitude;
            entityToUpdate.ProfundidadeMetros = coleta.ProfundidadeMetros;
            // Medições omitidas são preservadas; exclusões usam o endpoint de medições.
            foreach (var medicao in coleta.Medicoes)
            {
                var existente = entityToUpdate.Medicoes.SingleOrDefault(m => m.codigoMedicao == medicao.codigoMedicao);
                if (existente is null)
                    entityToUpdate.AdicionarMedicao(medicao);
                else
                    existente.Atualizar(medicao.codigoMedicao, medicao.valor, medicao.unidade,
                        medicao.censurado, medicao.limite);
            }
            context.SaveChanges();
        }

        public void Cadastrar(ColetaEntity coleta, int idCorpoHidrico)
        {
            ValidarCorpoHidrico(idCorpoHidrico);
            coleta.CorpoHidricoId = idCorpoHidrico;
            coleta.DataHora = coleta.DataHora.ToUniversalTime();
            context.Coletas.Add(coleta);
            context.SaveChanges();
        }

        public void CadastrarListaColetas(List<ColetaEntity> coletas)
        {
            foreach (var coleta in coletas)
            {
                ValidarCorpoHidrico(coleta.CorpoHidricoId);
                coleta.DataHora = coleta.DataHora.ToUniversalTime();
            }
            context.Coletas.AddRange(coletas);
            context.SaveChanges();
        }

        public void Deletar(int id)
        {
            var entityToDelete = context.Coletas.Find(id);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Coleta não encontrada");
            }
            context.Coletas.Remove(entityToDelete);
            context.SaveChanges();
        }

        public List<ColetaEntity> ObterPorCorpoHidrico(int corpoHidricoId)
        {
            return Consultar().Where(c => c.CorpoHidricoId == corpoHidricoId).ToList();
        }

        public ColetaEntity? ObterPorId(int id)
        {
            return Consultar().SingleOrDefault(c => c.Id == id);
        }

        public List<ColetaEntity> ObterPorPeriodo(
            int corpoHidricoId,
            string dataInicio,
            string dataFim
        )
        {
            return Consultar()
                .Where(c =>
                    c.CorpoHidricoId == corpoHidricoId
                    && c.DataHora >= DateTimeOffset.Parse(dataInicio).ToUniversalTime()
                    && c.DataHora<= DateTimeOffset.Parse(dataFim).ToUniversalTime()
                )
                .ToList();
        }

        private IQueryable<ColetaEntity> Consultar() => context.Coletas
            .AsNoTracking()
            .Include(c => c.Medicoes);

        private void ValidarCorpoHidrico(int id)
        {
            if (id <= 0 || !context.CorposHidricos.Any(c => c.Id == id))
                throw new ArgumentException("Corpo Hídrico não encontrado");
        }

        public List<ColetaEntity> ObterTodos()
        {
            return Consultar().ToList();
        }
    }
}
