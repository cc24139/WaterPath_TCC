using MediatR;

namespace back_end.src.Medicoes.Application;

public  record DeleteMedicaoCommand(int ColetaId, int MedicaoId) : IRequest<Unit>;
