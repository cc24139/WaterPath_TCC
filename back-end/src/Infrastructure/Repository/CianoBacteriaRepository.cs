using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.CianoBacteria;
using Infrastructure.Data;

namespace back_end.src.Infrastructure.Repository
{
    public class CianoBacteriaRepository : ICianoBacteriaRepository
    {
        private readonly WaterPathDbContext context;

        public CianoBacteriaRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public void Atualizar(CianoBacteriaEntity cianoBacteria)
        {
            var entityToUpdate = context.CianoBacterias.Find(cianoBacteria.Id);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Cianobactéria não encontrada");
            }
            entityToUpdate = new CianoBacteriaEntity(
                cianoBacteria.Tipo,
                cianoBacteria.Concentracao,
                cianoBacteria.Unidade
            );
            context.CianoBacterias.Update(entityToUpdate);
            context.SaveChanges();
        }

        public void Cadastrar(CianoBacteriaEntity cianoBacteria)
        {
            context.CianoBacterias.Add(cianoBacteria);
            context.SaveChanges();
        }

        public void Deletar(int id)
        {
            var entityToDelete = context.CianoBacterias.Find(id);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Cianobactéria não encontrada");
            }
            context.CianoBacterias.Remove(entityToDelete);
            context.SaveChanges();
        }

        public List<CianoBacteriaEntity> ObterTodos()
        {
            return context.CianoBacterias.ToList();
        }

        public CianoBacteriaEntity ObterPorId(int id)
        {
            return context.CianoBacterias.FirstOrDefault(c => c.Id == id);
        }
    }
}
