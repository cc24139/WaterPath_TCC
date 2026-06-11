namespace back_end.src.Domain.Codigo
{
    public interface ICodigoRepository
    {
        string GerarCodigo(CodigoEntity codigo);
        bool MarcarCodigoComoUsado(string emailUsuario, string codigo);
        bool VerificarPendenciaCodigo(string emailUsuario);
        void AlterarSenha(string email, string novaSenha);
    }
}
