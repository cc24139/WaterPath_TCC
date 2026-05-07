using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.QualidadeFutura;
using Infrastructure.Data;

namespace back_end.src.Infrastructure.Repository
{
    public class QualidadeFuturaRepository : IQualidadeFuturaRepository
    {
        private readonly WaterPathDbContext context;

        public QualidadeFuturaRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public void Atualizar(QualidadeFuturaEntity qualidadeFutura)
        {
            var entityToUpdate = context.QualidadesFuturas.Find(qualidadeFutura.Id);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Qualidade futura não encontrada");
            }
            context.QualidadesFuturas.Update(entityToUpdate);
            context.SaveChanges();
        }

        public void Cadastrar(QualidadeFuturaEntity qualidadeFutura)
        {
            context.QualidadesFuturas.Add(qualidadeFutura);
            context.SaveChanges();
        }

        public void Deletar(int id)
        {
            var entityToDelete = context.QualidadesFuturas.Find(id);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Qualidade futura não encontrada");
            }
            context.QualidadesFuturas.Remove(entityToDelete);
            context.SaveChanges();
        }

        public List<QualidadeFuturaEntity> ObterTodos()
        {
            return context.QualidadesFuturas.ToList();
        }

        public QualidadeFuturaEntity ObterPorId(int id)
        {
            return context.QualidadesFuturas.FirstOrDefault(q => q.Id == id);
        }
    }
}
