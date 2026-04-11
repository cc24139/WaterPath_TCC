namespace Infrastructure.Services;

public interface IEmailService
{
    void EnviarEmail(string destinatario, string assunto, string mensagem);
    void EnviarCodigoCadastro(string destinatario, string codigo);
    void EnviarCodigoEsqueciSenha(string destinatario, string codigo);
    void ReenviarCodigo(string destinatario, string codigo);
}