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

        public void Atualizar(ColetaEntity coleta)
        {
            var entityToUpdate = context.Coletas.Find(coleta.Id);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Coleta não encontrada");
            }
            context.Coletas.Update(entityToUpdate);
            context.SaveChanges();
        }

        public void Cadastrar(ColetaEntity coleta)
        {
            context.Coletas.Add(coleta);
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

        public List<ColetaEntity> ObterTodos()
        {
            return context.Coletas.ToList();
        }

        public ColetaEntity ObterPorId(int id)
        {
            return context.Coletas.FirstOrDefault(c => c.Id == id);
        }
    }
}
