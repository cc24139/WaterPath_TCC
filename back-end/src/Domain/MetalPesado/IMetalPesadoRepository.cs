using System.Collections.Generic;

namespace back_end.src.Domain.MetalPesado
{
    public interface IMetalPesadoRepository
    {
        void Cadastrar(MetalPesadoEntity metalPesado);
        void Atualizar(MetalPesadoEntity metalPesado);
        void Deletar(int id);
        MetalPesadoEntity ObterPorId(int id);
        List<MetalPesadoEntity> ObterTodos();
    }
}
