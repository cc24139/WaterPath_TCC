namespace back_end.src.Domain.Codigo
{
    public interface ICodigoRepository
    {
        string GerarCodigo(CodigoEntity codigo);
        bool MarcarCodigoComoUsado(int usuarioId, string codigo);
        bool VerificarPendenciaCodigo(int usuarioId);
    }
}
