using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.MetalPesado;
using Infrastructure.Data;

namespace back_end.src.Infrastructure.Repository
{
    public class MetalPesadoRepository : IMetalPesadoRepository
    {
        private readonly WaterPathDbContext context;

        public MetalPesadoRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public void Atualizar(MetalPesadoEntity metalPesado)
        {
            var entityToUpdate = context.MetaisPesados.Find(metalPesado.Id);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Metal pesado não encontrado");
            }
            entityToUpdate = new MetalPesadoEntity(
                metalPesado.Nome,
                metalPesado.Concentracao,
                metalPesado.Unidade
            );
            context.MetaisPesados.Update(entityToUpdate);
            context.SaveChanges();
        }

        public void Cadastrar(MetalPesadoEntity metalPesado)
        {
            context.MetaisPesados.Add(metalPesado);
            context.SaveChanges();
        }

        public void Deletar(int id)
        {
            var entityToDelete = context.MetaisPesados.Find(id);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Metal pesado não encontrado");
            }
            context.MetaisPesados.Remove(entityToDelete);
            context.SaveChanges();
        }

        public List<MetalPesadoEntity> ObterTodos()
        {
            return context.MetaisPesados.ToList();
        }

        public MetalPesadoEntity ObterPorId(int id)
        {
            return context.MetaisPesados.FirstOrDefault(m => m.Id == id);
        }
    }
}
