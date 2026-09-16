using back_end.src.Medicoes.Application;
using MediatR;

namespace back_end.src.Medicoes.Application;

public  record UpdateMedicaoCommand(
    int ColetaId,
    int MedicaoId,
    InputMedicao Medicao) : IRequest<Unit>;
