using MediatR;

namespace Application.Commands.Codigo;

public class CommandGerarCodigo : IRequest<string>
{
    public string UsuarioEmail { get; set; }
    public string Codigo { get; set; } = string.Empty;

    public CommandGerarCodigo() { }

    public CommandGerarCodigo(string usuarioEmail)
    {
        UsuarioEmail = usuarioEmail;
    }

}

public class CommandValidarCodigo : IRequest<string>
{
    public string UsuarioEmail { get; set; }
    public string Codigo { get; set; } = string.Empty;

    public CommandValidarCodigo() { }

    public CommandValidarCodigo(string usuarioEmail, string codigo)
    {
        UsuarioEmail = usuarioEmail;
        Codigo = codigo;
    }
}
