using System;
using System.Linq;
using back_end.src.Domain.Codigo;
using Infrastructure.Data;

namespace back_end.src.Infrastructure.Repository
{
    public class CodigoRepository : ICodigoRepository
    {
        private readonly WaterPathDbContext context;

        public CodigoRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public void GerarCodigo(CodigoEntity codigo)
        {
            context.Codigos.Add(codigo);
            context.SaveChanges();
        }

        public bool MarcarCodigoComoUsado(int usuarioId, string codigo)
        {
            var codigoEntity = context.Codigos.FirstOrDefault(c =>
                c.UsuarioId == usuarioId
                && c.Codigo == codigo
                && !c.Usado
                && c.DataExpiracao > DateTime.UtcNow
            );

            if (codigoEntity == null)
            {
                return false;
            }

            codigoEntity.Usado = true;
            context.Codigos.Update(codigoEntity);
            context.SaveChanges();
            return true;
        }

        public bool VerificarPendenciaCodigo(int usuarioId)
        {
            return context.Codigos.Any(c =>
                c.UsuarioId == usuarioId && !c.Usado && c.DataExpiracao > DateTime.UtcNow
            );
        }
    }
}
