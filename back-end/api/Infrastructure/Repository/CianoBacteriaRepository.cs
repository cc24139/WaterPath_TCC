using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.CianoBacteria;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.Infrastructure.Repository
{
    public class CianoBacteriaRepository : ICianoBacteriaRepository
    {
        private readonly WaterPathDbContext context;

        public CianoBacteriaRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public async Task Atualizar(CianoBacteriaEntity cianoBacteria, CancellationToken cancellationToken = default)
        {
            var entityToUpdate = await context.CianoBacterias.FindAsync([cianoBacteria.Id], cancellationToken);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Cianobactéria não encontrada");
            }
            entityToUpdate = new CianoBacteriaEntity(
                cianoBacteria.Tipo,
                cianoBacteria.Concentracao,
                cianoBacteria.Unidade,
                cianoBacteria.Coleta
            );
            context.CianoBacterias.Update(entityToUpdate);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Cadastrar(CianoBacteriaEntity cianoBacteria, CancellationToken cancellationToken = default)
        {
            await context.CianoBacterias.AddAsync(cianoBacteria, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Deletar(int id, CancellationToken cancellationToken = default)
        {
            var entityToDelete = await context.CianoBacterias.FindAsync([id], cancellationToken);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Cianobactéria não encontrada");
            }
            context.CianoBacterias.Remove(entityToDelete);
            await context.SaveChangesAsync(cancellationToken);
        }

        public Task<List<CianoBacteriaEntity>> ObterTodos(CancellationToken cancellationToken = default)
        {
            return context.CianoBacterias.ToListAsync(cancellationToken);
        }

        public Task<CianoBacteriaEntity?> ObterPorId(int id, CancellationToken cancellationToken = default)
        {
            return context.CianoBacterias.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }
    }
}
