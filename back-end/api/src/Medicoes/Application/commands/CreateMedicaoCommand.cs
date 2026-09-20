
using back_end.src.Medicoes.Application;
using MediatR;

namespace back_end.src.Medicoes.Application;

public class CreateMedicaoCommand : IRequest<Unit>
{
    public int ColetaId { get; set; }
    public InputMedicao medicao { get; set; } = null!;
    public CreateMedicaoCommand() { }
    public CreateMedicaoCommand(int coletaId, InputMedicao medicao)
    {
        ColetaId = coletaId;
        this.medicao = medicao;
    }
}
