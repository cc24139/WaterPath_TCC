import type {
  AnalysisSortOption,
  AnalysisStatusFilter,
  UserAnalysis,
} from "@/features/my-analyses/types/myAnalyses";

export const analysesPerPage = 5;

export const statusFilterOptions: Array<{
  label: AnalysisStatusFilter;
  value: AnalysisStatusFilter;
}> = [
  { label: "Todos os status", value: "Todos os status" },
  { label: "Processando", value: "Processando" },
  { label: "Concluída", value: "Concluída" },
  { label: "Atenção", value: "Atenção" },
  { label: "Rascunho", value: "Rascunho" },
];

export const sortOptions: Array<{
  label: string;
  value: AnalysisSortOption;
}> = [
  { label: "Mais recentes", value: "newest" },
  { label: "Mais antigas", value: "oldest" },
  { label: "Nome do corpo hídrico", value: "water-body" },
  { label: "Status", value: "status" },
];

export const mockUserAnalyses: UserAnalysis[] = [
  {
    id: "analysis-001",
    waterBodyId: "rio-atibaia",
    waterBodyName: "Rio Atibaia",
    city: "Atibaia",
    state: "SP",
    collectionPoint: "Ponte da Av. Lucas Nogueira Garcez",
    analyzedAt: "2026-04-12T10:30:00",
    status: "Processando",
  },
  {
    id: "analysis-002",
    waterBodyId: "lagoa-piratininga",
    waterBodyName: "Lagoa de Piratininga",
    city: "Niterói",
    state: "RJ",
    collectionPoint: "Itaipu – Próx. ao Clube Naval",
    analyzedAt: "2026-04-08T14:15:00",
    status: "Concluída",
  },
  {
    id: "analysis-003",
    waterBodyId: "rio-jaguari",
    waterBodyName: "Rio Jaguari",
    city: "Jaguariúna",
    state: "SP",
    collectionPoint: "Captação – SAAE Jaguariúna",
    analyzedAt: "2026-04-05T09:45:00",
    status: "Atenção",
  },
  {
    id: "analysis-004",
    waterBodyId: "rio-das-velhas",
    waterBodyName: "Rio das Velhas",
    city: "Belo Horizonte",
    state: "MG",
    collectionPoint: "Ponte do Bairro São Francisco",
    analyzedAt: "2026-04-01T11:20:00",
    status: "Concluída",
  },
  {
    id: "analysis-005",
    waterBodyId: "represa-guarapiranga",
    waterBodyName: "Represa Guarapiranga",
    city: "São Paulo",
    state: "SP",
    collectionPoint: "Braço do Parelheiros – Ponto 3",
    analyzedAt: "2026-03-28T16:05:00",
    status: "Rascunho",
  },
  {
    id: "analysis-006",
    waterBodyId: "rio-tiete",
    waterBodyName: "Rio Tietê",
    city: "São Paulo",
    state: "SP",
    collectionPoint: "Ponte dos Remédios",
    analyzedAt: "2026-03-22T08:50:00",
    status: "Processando",
  },
  {
    id: "analysis-007",
    waterBodyId: "lago-paranoa",
    waterBodyName: "Lago Paranoá",
    city: "Brasília",
    state: "DF",
    collectionPoint: "Pier da Ermida Dom Bosco",
    analyzedAt: "2026-03-18T13:10:00",
    status: "Concluída",
  },
  {
    id: "analysis-008",
    waterBodyId: "rio-capibaribe",
    waterBodyName: "Rio Capibaribe",
    city: "Recife",
    state: "PE",
    collectionPoint: "Ponte da Capunga",
    analyzedAt: "2026-03-12T15:40:00",
    status: "Concluída",
  },
  {
    id: "analysis-009",
    waterBodyId: "lagoa-rodrigo-de-freitas",
    waterBodyName: "Lagoa Rodrigo de Freitas",
    city: "Rio de Janeiro",
    state: "RJ",
    collectionPoint: "Deck dos Pedalinhos",
    analyzedAt: "2026-03-08T07:35:00",
    status: "Atenção",
  },
  {
    id: "analysis-010",
    waterBodyId: "rio-iguacu",
    waterBodyName: "Rio Iguaçu",
    city: "Curitiba",
    state: "PR",
    collectionPoint: "Parque Náutico",
    analyzedAt: "2026-03-03T10:25:00",
    status: "Processando",
  },
  {
    id: "analysis-011",
    waterBodyId: "rio-guaiba",
    waterBodyName: "Lago Guaíba",
    city: "Porto Alegre",
    state: "RS",
    collectionPoint: "Orla do Gasômetro",
    analyzedAt: "2026-02-25T17:15:00",
    status: "Concluída",
  },
  {
    id: "analysis-012",
    waterBodyId: "rio-paraiba-do-sul",
    waterBodyName: "Rio Paraíba do Sul",
    city: "Campos dos Goytacazes",
    state: "RJ",
    collectionPoint: "Ponte Barcelos Martins",
    analyzedAt: "2026-02-19T09:00:00",
    status: "Rascunho",
  },
];
