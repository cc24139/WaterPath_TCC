using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.Imagem;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.Infrastructure.Repository
{
    public class ImagemRepository : IImagemRepository
    {
        private readonly WaterPathDbContext context;

        public ImagemRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public async Task Atualizar(ImagemEntity imagem, CancellationToken cancellationToken = default)
        {
            var entityToUpdate = await context.Imagens.FindAsync([imagem.Id], cancellationToken);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Imagem não encontrada");
            }
            context.Imagens.Update(entityToUpdate);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Cadastrar(ImagemEntity imagem, CancellationToken cancellationToken = default)
        {
            await context.Imagens.AddAsync(imagem, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Deletar(int id, CancellationToken cancellationToken = default)
        {
            var entityToDelete = await context.Imagens.FindAsync([id], cancellationToken);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Imagem não encontrada");
            }
            context.Imagens.Remove(entityToDelete);
            await context.SaveChangesAsync(cancellationToken);
        }

        public Task<List<ImagemEntity>> ObterTodos(CancellationToken cancellationToken = default)
        {
            return context.Imagens.ToListAsync(cancellationToken);
        }

        public async Task<ImagemEntity?> ObterPorId(int id, CancellationToken cancellationToken = default)
        {
            return await context.Imagens.FindAsync([id], cancellationToken);
        }

        public virtual Task<List<ImagemEntity>> ObterImagensPorCorpoHidrico(int corpoHidricoId, CancellationToken cancellationToken = default)
        {
            return context.Imagens.Where(i => i.CorpoHidrico.Id == corpoHidricoId).ToListAsync(cancellationToken);
        }

        public virtual Task<List<ImagemEntity>> ObterImagensPorColeta(int coletaId, CancellationToken cancellationToken = default)
        {
            return context.Imagens.Where(i => i.Coleta.Id == coletaId).ToListAsync(cancellationToken);
        }

        public virtual Task<List<ImagemEntity>> ObterImagensPorPeriodo(
            int corpoHidricoId,
            string dataInicio,
            string dataFim,
            CancellationToken cancellationToken = default
        )
        {
            return context
                .Imagens.Where(i =>
                    i.CorpoHidrico.Id == corpoHidricoId
                    && i.DataUpload >= DateTime.Parse(dataInicio)
                    && i.DataUpload <= DateTime.Parse(dataFim)
                )
                .ToListAsync(cancellationToken);
        }
    }
}
