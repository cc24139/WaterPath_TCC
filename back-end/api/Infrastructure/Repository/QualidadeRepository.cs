using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.Qualidade;
using Infrastructure.Data;

namespace back_end.src.Infrastructure.Repository
{
    public class QualidadeRepository : IQualidadeRepository
    {
        private readonly WaterPathDbContext context;

        public QualidadeRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public void Atualizar(QualidadeEntity qualidade)
        {
            var entityToUpdate = context.Qualidades.Find(qualidade.Id);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Qualidade não encontrada");
            }
            context.Qualidades.Update(entityToUpdate);
            context.SaveChanges();
        }

        public void Cadastrar(QualidadeEntity qualidade)
        {
            context.Qualidades.Add(qualidade);
            context.SaveChanges();
        }

        public void Deletar(int id)
        {
            var entityToDelete = context.Qualidades.Find(id);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Qualidade não encontrada");
            }
            context.Qualidades.Remove(entityToDelete);
            context.SaveChanges();
        }

        public List<QualidadeEntity> ObterTodos()
        {
            return context.Qualidades.ToList();
        }

        public QualidadeEntity ObterPorId(int id)
        {
            return context.Qualidades.FirstOrDefault(q => q.Id == id);
        }
    }
}
