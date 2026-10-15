namespace back_end.src.Medicoes.Domain;

public interface IMedicoesRepository
{
    Task Cadastrar(MedicoesEntity medicao, int coletaId, CancellationToken cancellationToken = default);
    Task Atualizar(MedicoesEntity medicao, int coletaId, int medicaoId, CancellationToken cancellationToken = default);
    Task Deletar(int coletaId, int medicaoId, CancellationToken cancellationToken = default);
    Task<MedicoesEntity?> ObterPorId(int coletaId, int medicaoId, CancellationToken cancellationToken = default);
    Task<List<MedicoesEntity>> ObterPorColeta(int coletaId, CancellationToken cancellationToken = default);
    Task<List<MedicoesEntity>> ObterTodos(CancellationToken cancellationToken = default);
}
