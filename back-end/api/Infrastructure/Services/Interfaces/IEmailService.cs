namespace Infrastructure.Services;

public interface IEmailService
{
    Task EnviarEmail(string destinatario, string assunto, string mensagem);
    Task EnviarCodigoCadastro(string destinatario, string codigo);
    Task EnviarCodigoEsqueciSenha(string destinatario, string codigo);
    Task ReenviarCodigo(string destinatario, string codigo);
}