using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.Imagem;
using Infrastructure.Data;

namespace back_end.src.Infrastructure.Repository
{
    public class ImagemRepository : IImagemRepository
    {
        private readonly WaterPathDbContext context;

        public ImagemRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public void Atualizar(ImagemEntity imagem)
        {
            var entityToUpdate = context.Imagens.Find(imagem.Id);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Imagem não encontrada");
            }
            context.Imagens.Update(entityToUpdate);
            context.SaveChanges();
        }

        public void Cadastrar(ImagemEntity imagem)
        {
            context.Imagens.Add(imagem);
            context.SaveChanges();
        }

        public void Deletar(int id)
        {
            var entityToDelete = context.Imagens.Find(id);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Imagem não encontrada");
            }
            context.Imagens.Remove(entityToDelete);
            context.SaveChanges();
        }

        public List<ImagemEntity> ObterTodos()
        {
            return context.Imagens.ToList();
        }

        public ImagemEntity ObterPorId(int id)
        {
            return context.Imagens.Find(id);
        }

        public virtual List<ImagemEntity> ObterImagensPorCorpoHidrico(int corpoHidricoId)
        {
            return context.Imagens.Where(i => i.CorpoHidrico.Id == corpoHidricoId).ToList();
        }

        public virtual List<ImagemEntity> ObterImagensPorColeta(int coletaId)
        {
            return context.Imagens.Where(i => i.Coleta.Id == coletaId).ToList();
        }

        public virtual List<ImagemEntity> ObterImagensPorPeriodo(
            int corpoHidricoId,
            string dataInicio,
            string dataFim
        )
        {
            return context
                .Imagens.Where(i =>
                    i.CorpoHidrico.Id == corpoHidricoId
                    && i.DataUpload >= DateTime.Parse(dataInicio)
                    && i.DataUpload <= DateTime.Parse(dataFim)
                )
                .ToList();
        }
    }
}
