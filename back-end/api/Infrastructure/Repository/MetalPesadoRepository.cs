using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.MetalPesado;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.Infrastructure.Repository
{
    public class MetalPesadoRepository : IMetalPesadoRepository
    {
        private readonly WaterPathDbContext context;

        public MetalPesadoRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public async Task Atualizar(MetalPesadoEntity metalPesado, CancellationToken cancellationToken = default)
        {
            var entityToUpdate = await context.MetaisPesados.FindAsync([metalPesado.Id], cancellationToken);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Metal pesado não encontrado");
            }
            entityToUpdate = new MetalPesadoEntity(
                metalPesado.Nome,
                metalPesado.Concentracao,
                metalPesado.Unidade,
                metalPesado.Coleta
            );
            context.MetaisPesados.Update(entityToUpdate);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Cadastrar(MetalPesadoEntity metalPesado, CancellationToken cancellationToken = default)
        {
            await context.MetaisPesados.AddAsync(metalPesado, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Deletar(int id, CancellationToken cancellationToken = default)
        {
            var entityToDelete = await context.MetaisPesados.FindAsync([id], cancellationToken);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Metal pesado não encontrado");
            }
            context.MetaisPesados.Remove(entityToDelete);
            await context.SaveChangesAsync(cancellationToken);
        }

        public Task<List<MetalPesadoEntity>> ObterTodos(CancellationToken cancellationToken = default)
        {
            return context.MetaisPesados.ToListAsync(cancellationToken);
        }

        public async Task<MetalPesadoEntity?> ObterPorId(int id, CancellationToken cancellationToken = default)
        {
            return await context.MetaisPesados.FindAsync([id], cancellationToken);
        }
    }
}
