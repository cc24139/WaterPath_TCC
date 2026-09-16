using back_end.src.Medicoes.Domain;
using MediatR;

namespace back_end.src.Medicoes.Application.Queries;

public record QueryObterMedicaoPorId(int ColetaId, int MedicaoId) : IRequest<MedicoesEntity?>;
public record QueryObterMedicoesPorColeta(int ColetaId) : IRequest<List<MedicoesEntity>>;
public record QueryObterTodasMedicoes : IRequest<List<MedicoesEntity>>;
