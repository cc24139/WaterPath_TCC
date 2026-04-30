using MediatR;

namespace Application.Commands.Codigo;

public class CommandGerarCodigo : IRequest<string>
{
    public int UsuarioId { get; set; }
    public string Codigo { get; set; } = string.Empty;

    public CommandGerarCodigo() { }

    public CommandGerarCodigo(int usuarioId)
    {
        UsuarioId = usuarioId;
    }
}

public class CommandValidarCodigo : IRequest<string>
{
    public int UsuarioId { get; set; }
    public string Codigo { get; set; } = string.Empty;

    public CommandValidarCodigo() { }

    public CommandValidarCodigo(int usuarioId, string codigo)
    {
        UsuarioId = usuarioId;
        Codigo = codigo;
    }
}
