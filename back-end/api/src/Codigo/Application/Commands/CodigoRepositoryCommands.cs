using back_end.src.Domain.Codigo;
using MediatR;

namespace Application.Commands.Codigo;

public class CommandGerarCodigoRepository : IRequest<string>
{
    public CodigoEntity Codigo { get; set; } = new();
}

public class CommandMarcarCodigoComoUsado : IRequest<bool>
{
    public string UsuarioEmail { get; set; }
    public string Codigo { get; set; } = string.Empty;
}
