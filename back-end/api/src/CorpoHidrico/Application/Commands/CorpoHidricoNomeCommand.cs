
using back_end.src.Domain.CorpoHidrico;
using MediatR;

public class CorpoHidricoNomeCommand : IRequest<CorpoHidricoEntity?>
{
    public string Nome { get; set; } = string.Empty;
}