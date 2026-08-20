using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Infrastructure.Services;

namespace back_end.src.Infrastructure.Services
{
    public class EmailServices : IEmailService
    {
        private const string Remetente = "gurtgigachaduni.zazu@gmail.com";
        private readonly SmtpClient _smtpServer;

        public EmailServices()
        {
            _smtpServer = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new System.Net.NetworkCredential(
                    Remetente,
                    Environment.GetEnvironmentVariable("senhaEmail")
                ),
                EnableSsl = true,
                Timeout = 30000
            };
        }

        public async Task EnviarCodigoCadastro(string destinatario, string codigo)
        {
        
            using var mail = new MailMessage
            {
                From = new MailAddress(Remetente),
                Subject = "Código de Cadastro",
                Body = $"Seu código de cadastro é: {codigo}",
                IsBodyHtml = false
            };
            mail.To.Add(destinatario);
            await _smtpServer.SendMailAsync(mail);
        }

        public async Task EnviarCodigoEsqueciSenha(string destinatario, string codigo)
        {
            using var mail = new MailMessage(
                from: Remetente,
                to: destinatario,
                subject: "Código de Esqueci Senha",
                body: $"Seu código de esqueci senha é: {codigo}"
            );
            await _smtpServer.SendMailAsync(mail);
        }

        public async Task EnviarEmail(string destinatario, string assunto, string mensagem)
        {
            using var mail = new MailMessage(
                from: Remetente,
                to: destinatario,
                subject: assunto,
                body: mensagem
            );
            await _smtpServer.SendMailAsync(mail);
        }

        public async Task ReenviarCodigo(string destinatario, string codigo)
        {
            using var mail = new MailMessage(
                from: Remetente,
                to: destinatario,
                subject: "Reenvio de Código",
                body: $"Seu código reenviado é: {codigo}"
            );
            await _smtpServer.SendMailAsync(mail);
        }
    }
}
