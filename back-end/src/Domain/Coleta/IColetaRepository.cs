using System.Collections.Generic;

namespace back_end.src.Domain.Coleta
{
    public interface IColetaRepository
    {
        void Cadastrar(ColetaEntity coleta, int idCorpoHidrico);
        void Atualizar(ColetaEntity coleta, int idColeta);
        void Deletar(int id);
        ColetaEntity ObterPorId(int id);
        List<ColetaEntity> ObterTodos();
        void CadastrarListaColetas(List<ColetaEntity> coletas);
        void ObterPorPeriodo(int corpoHidricoId, string dataInicio, string dataFim);
        void ObterPorCorpoHidrico(int corpoHidricoId);
    }
}
