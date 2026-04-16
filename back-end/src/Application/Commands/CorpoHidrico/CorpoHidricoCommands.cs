using back_end.src.Domain.CorpoHidrico;
using MediatR;

namespace Application.Commands.CorpoHidrico;

public class CommandCadastrarCorpoHidrico : IRequest<Unit>
{
    public string Nome { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public double Tamanho { get; set; }
    public bool EhPrivado { get; set; }
}

public class CommandAtualizarCorpoHidrico : IRequest<Unit>
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public double Tamanho { get; set; }
    public bool EhPrivado { get; set; }
}

public class CommandDeletarCorpoHidrico : IRequest<Unit>
{
    public int Id { get; set; }
}

public class CommandAdicionarUsuario : IRequest<Unit>
{
    public int CorpoHidricoId { get; set; } = new();
    public int UserId { get; set; } = new();
}

public class CommandDeletarUsuario : IRequest<Unit>
{
    public int CorpoHidricoId { get; set; } = new();
    public int UserId { get; set; } = new();
}
