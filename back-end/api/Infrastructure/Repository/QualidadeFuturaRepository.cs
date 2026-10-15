using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.QualidadeFutura;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.Infrastructure.Repository
{
    public class QualidadeFuturaRepository : IQualidadeFuturaRepository
    {
        private readonly WaterPathDbContext context;

        public QualidadeFuturaRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public async Task Atualizar(QualidadeFuturaEntity qualidadeFutura, CancellationToken cancellationToken = default)
        {
            var entityToUpdate = await context.QualidadesFuturas.FindAsync([qualidadeFutura.Id], cancellationToken);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Qualidade futura não encontrada");
            }
            context.QualidadesFuturas.Update(entityToUpdate);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Cadastrar(QualidadeFuturaEntity qualidadeFutura, CancellationToken cancellationToken = default)
        {
            await context.QualidadesFuturas.AddAsync(qualidadeFutura, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Deletar(int id, CancellationToken cancellationToken = default)
        {
            var entityToDelete = await context.QualidadesFuturas.FindAsync([id], cancellationToken);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Qualidade futura não encontrada");
            }
            context.QualidadesFuturas.Remove(entityToDelete);
            await context.SaveChangesAsync(cancellationToken);
        }

        public Task<List<QualidadeFuturaEntity>> ObterTodos(CancellationToken cancellationToken = default)
        {
            return context.QualidadesFuturas.ToListAsync(cancellationToken);
        }

        public Task<QualidadeFuturaEntity?> ObterPorId(int id, CancellationToken cancellationToken = default)
        {
            return context.QualidadesFuturas.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
        }
    }
}
