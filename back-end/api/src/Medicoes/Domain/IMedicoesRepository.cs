namespace back_end.src.Medicoes.Domain;

public interface IMedicoesRepository
{
    void Cadastrar(MedicoesEntity medicao, int coletaId);
    void Atualizar(MedicoesEntity medicao, int coletaId, int medicaoId);
    void Deletar(int coletaId, int medicaoId);
    MedicoesEntity? ObterPorId(int coletaId, int medicaoId);
    List<MedicoesEntity> ObterPorColeta(int coletaId);
    List<MedicoesEntity> ObterTodos();
}
