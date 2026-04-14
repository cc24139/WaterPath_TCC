using back_end.src.Domain.MetalPesado;
using MediatR;

namespace Application.Commands.MetalPesado;

public class CommandCadastrarMetalPesado : IRequest<Unit>
{
    public MetalPesadoEntity MetalPesado { get; set; } = new();
}

public class CommandAtualizarMetalPesado : IRequest<Unit>
{
    public MetalPesadoEntity MetalPesado { get; set; } = new();
}

public class CommandDeletarMetalPesado : IRequest<Unit>
{
    public int Id { get; set; }
}
