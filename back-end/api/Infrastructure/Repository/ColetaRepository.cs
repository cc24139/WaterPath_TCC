using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.Coleta;
using Infrastructure.Data;

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
            var entityToUpdate = context.Coletas.Find(idColeta);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Coleta não encontrada");
            }
            context.Coletas.Update(entityToUpdate);
            context.SaveChanges();
        }

        public void Cadastrar(ColetaEntity coleta, int idCorpoHidrico)
        {
            var corpoHidrico = context.CorposHidricos.Find(idCorpoHidrico);
            if (corpoHidrico == null)
            {
                throw new ArgumentException("Corpo Hídrico não encontrado");
            }

            coleta.CorpoHidrico = corpoHidrico;
            context.Coletas.Add(coleta);
            context.SaveChanges();
        }

        public void CadastrarListaColetas(List<ColetaEntity> coletas)
        {
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
            return context.Coletas.Where(c => c.CorpoHidrico.Id == corpoHidricoId).ToList();
        }

        public ColetaEntity ObterPorId(int id)
        {
            return context.Coletas.Find(id);
        }

        public List<ColetaEntity> ObterPorPeriodo(
            int corpoHidricoId,
            string dataInicio,
            string dataFim
        )
        {
            return context
                .Coletas.Where(c =>
                    c.CorpoHidrico.Id == corpoHidricoId
                    && c.Data >= DateTime.Parse(dataInicio)
                    && c.Data <= DateTime.Parse(dataFim)
                )
                .ToList();
        }

        public List<ColetaEntity> ObterTodos()
        {
            return context.Coletas.ToList();
        }
    }
}
