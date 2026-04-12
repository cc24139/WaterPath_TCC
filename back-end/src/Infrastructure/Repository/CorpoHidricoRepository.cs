using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.CorpoHidrico;
using Infrastructure.Data;

namespace back_end.src.Infrastructure.Repository
{
    public class CorpoHidricoRepository : ICorpoHidricoRepository
    {
        private readonly WaterPathDbContext context;

        public CorpoHidricoRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public void Atualizar(CorpoHidricoEntity corpoHidrico)
        {
            var entityToUpdate = context.CorposHidricos.Find(corpoHidrico.Id);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Corpo hídrico não encontrado");
            }
            context.CorposHidricos.Update(corpoHidrico);
            context.SaveChanges();
        }

        public void Cadastrar(CorpoHidricoEntity corpoHidrico)
        {
            context.CorposHidricos.Add(corpoHidrico);
            context.SaveChanges();
        }

        public void Deletar(int id)
        {
            var entityToDelete = context.CorposHidricos.Find(id);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Corpo hídrico não encontrado");
            }
            context.CorposHidricos.Remove(entityToDelete);
            context.SaveChanges();
        }

        public List<CorpoHidricoEntity> ObterTodos()
        {
            return context.CorposHidricos.ToList();
        }

        public CorpoHidricoEntity ObterCorpoHidricoPorId(int id)
        {
            return context.CorposHidricos.FirstOrDefault(c => c.Id == id);
        }
    }
}
