import { buildRivers } from "../utils/riverData";

// Used only by /water-bodies/demo. These records are never sent to the API.
const bodies = [
  { id: 9001, nome: "Rio Aurora", localizacao: "Vale Azul — exemplo" },
  { id: 9002, nome: "Ribeirão das Pedras", localizacao: "Serra Verde — exemplo" },
  { id: 9003, nome: "Lago Sereno", localizacao: "Vale Azul — exemplo" },
  { id: 9004, nome: "Córrego do Vale", localizacao: "Serra Verde — exemplo" },
  { id: 9005, nome: "Rio das Palmeiras", localizacao: "Costa Clara — exemplo" },
  { id: 9006, nome: "Lagoa do Horizonte", localizacao: "Costa Clara — exemplo" },
];

const dates = [
  "2026-01-12", "2026-02-03", "2026-03-19", "2026-04-08",
  "2026-05-27", "2026-06-15", "2026-08-04", "2026-09-02",
];

type Sample = [ph: number | null, conductivity: number | null, oxygen: number | null];

function sample(id: number, bodyId: number, date: string, [ph, conductivity, oxygen]: Sample) {
  return {
    id, corpoHidricoId: bodyId, dataHora: date,
    medicoes: [
      { codigoMedicao: "Ph", valor: ph, unidade: "pH", censurado: false },
      { codigoMedicao: "CondutividadeEletrica", valor: conductivity, unidade: "µS/cm", censurado: false },
      { codigoMedicao: "OxigenioDissolvido", valor: oxygen, unidade: "mg/L", censurado: false },
    ],
  };
}

function collections(bodyId: number, samples: Sample[]) {
  return samples.map((values, index) => sample(bodyId * 100 + index + 1, bodyId, `${dates[index]}T10:00:00Z`, values));
}

const samples = [
  ...collections(9001, [
    [6.7, 180, 5.3], [7.0, 195, 5.7], [7.3, 210, 5.4], [7.1, 205, 6.1],
    [6.9, 190, 6.4], [7.2, 215, 5.9], [7.4, 200, 6.8], [7.2, 185, 7.1],
  ]),
  ...collections(9002, [
    [6.3, 320, 4.1], [6.6, 360, 3.8], [null, 410, 3.2], [6.1, null, 3.5],
    [6.5, 390, null], [6.8, 355, 4.4], [7.0, 340, 4.8], [6.9, null, 5.0],
  ]),
  sample(900301, 9003, "2026-09-01T09:30:00Z", [7.4, 95, 8.2]),
  ...collections(9004, [
    [7.2, 250, 5.5], [7.0, 310, 5.0], [6.5, 380, 4.2], [6.1, 450, 3.4],
    [5.8, 520, 2.9], [5.5, 580, 2.3], [5.3, 620, 1.8], [5.6, 595, 2.1],
  ]),
  ...collections(9005, [[7.0, 160, 6.0], [7.2, 170, 6.4], [7.1, 165, 5.8]]),
];

const qualities = [
  { id: 1, corpoHidrico: { id: 9001 }, iqa: 82 },
  { id: 2, corpoHidrico: { id: 9002 }, iqa: 58 },
  { id: 3, corpoHidrico: { id: 9003 }, iqa: 94 },
  { id: 4, corpoHidrico: { id: 9004 }, iqa: 38 },
  { id: 5, corpoHidrico: { id: 9005 }, iqa: 68 },
  { id: 6, corpoHidrico: { id: 9005 }, iqa: 76 },
];

// The preview uses the same normalization and chart rendering as the live page.
export const demoRivers = buildRivers(bodies, samples, qualities).rivers;
