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

type Sample = [ph: number | null, turbidity: number | null, oxygen: number | null];

function collections(bodyId: number, samples: Sample[]) {
  return samples.map(([ph, turbidez, oxigenioDissolvido], index) => ({
    id: bodyId * 100 + index + 1,
    corpoHidrico: { id: bodyId },
    data: `${dates[index]}T10:00:00Z`,
    ph,
    turbidez,
    oxigenioDissolvido,
  }));
}

const samples = [
  ...collections(9001, [
    [6.7, 12, 5.3], [7.0, 9, 5.7], [7.3, 14, 5.4], [7.1, 8, 6.1],
    [6.9, 6, 6.4], [7.2, 10, 5.9], [7.4, 5, 6.8], [7.2, 4, 7.1],
  ]),
  ...collections(9002, [
    [6.3, 25, 4.1], [6.6, 32, 3.8], [null, 40, 3.2], [6.1, null, 3.5],
    [6.5, 28, null], [6.8, 21, 4.4], [7.0, 18, 4.8], [6.9, null, 5.0],
  ]),
  { id: 900301, corpoHidrico: { id: 9003 }, data: "2026-09-01T09:30:00Z", ph: 7.4, turbidez: 2.1, oxigenioDissolvido: 8.2 },
  ...collections(9004, [
    [7.2, 18, 5.5], [7.0, 24, 5.0], [6.5, 38, 4.2], [6.1, 52, 3.4],
    [5.8, 64, 2.9], [5.5, 80, 2.3], [5.3, 92, 1.8], [5.6, 85, 2.1],
  ]),
  ...collections(9005, [[7.0, 10, 6.0], [7.2, 8, 6.4], [7.1, 12, 5.8]]),
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
