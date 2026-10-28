import type { IconType } from "react-icons";
import {
  LuAtom,
  LuBrainCircuit,
  LuChartNoAxesCombined,
  LuClipboardCheck,
  LuDroplet,
  LuEye,
  LuFlaskConical,
  LuLeaf,
  LuThermometer,
  LuWaves,
  LuWind,
  LuZap,
} from "react-icons/lu";

import { indicatorFields } from "@/features/add-analysis/constants/addAnalysisOptions";
import type { IndicatorName } from "@/features/add-analysis/types/addAnalysis";

export const methodologySteps = [
  {
    title: "Coleta",
    description: "Registre o corpo hídrico, a data e as medições obtidas em campo ou laboratório.",
    icon: LuDroplet,
  },
  {
    title: "Validação",
    description: "Os campos obrigatórios, os valores e as unidades são conferidos antes do envio.",
    icon: LuClipboardCheck,
  },
  {
    title: "Estimativas com IA",
    description: "Ao solicitar a análise, modelos usam os parâmetros da coleta para estimar metais.",
    icon: LuBrainCircuit,
  },
  {
    title: "Visão computacional",
    description: "A imagem é analisada para identificar sinais visuais e apresentar as detecções.",
    icon: LuEye,
  },
  {
    title: "Resultados",
    description: "Medições, estimativas e motivos de risco ficam disponíveis para acompanhamento.",
    icon: LuChartNoAxesCombined,
  },
];

interface IndicatorExplanation {
  symbol: string;
  icon: IconType;
  summary: string;
  description: string;
  usage: string;
}

const indicatorExplanations: Record<IndicatorName, IndicatorExplanation> = {
  temperatura: {
    symbol: "T",
    icon: LuThermometer,
    summary: "O contexto térmico da água.",
    description: "A temperatura influencia processos físicos e biológicos e a disponibilidade de oxigênio na água. Ela pode variar ao longo do dia e entre as estações.",
    usage: "Ajuda a contextualizar a coleta e compõe as entradas dos modelos de estimativa de metais.",
  },
  ph: {
    symbol: "pH",
    icon: LuFlaskConical,
    summary: "Acidez e alcalinidade em uma mesma escala.",
    description: "O pH descreve o caráter ácido ou alcalino da água. Suas alterações podem afetar organismos aquáticos e o comportamento de substâncias presentes no ambiente.",
    usage: "Participa das estimativas de metais e dos critérios de triagem do nível de risco.",
  },
  condutividade_eletrica: {
    symbol: "EC",
    icon: LuZap,
    summary: "Uma leitura relacionada aos íons dissolvidos.",
    description: "Expressa a capacidade da água de conduzir corrente elétrica, relacionada à presença de íons dissolvidos. Sozinha, não identifica quais substâncias estão presentes.",
    usage: "É usada nas estimativas de metais e pode ser acompanhada no histórico de medições.",
  },
  oxigenio_dissolvido: {
    symbol: "OD",
    icon: LuWind,
    summary: "Oxigênio disponível para a vida aquática.",
    description: "Representa a concentração de oxigênio dissolvido na água. Sua disponibilidade está ligada às condições do ambiente e aos processos de respiração e decomposição.",
    usage: "Participa das estimativas de metais e dos critérios de triagem do nível de risco.",
  },
  solidos_suspensos_totais: {
    symbol: "TSS",
    icon: LuWaves,
    summary: "Partículas que permanecem em suspensão.",
    description: "Quantifica os sólidos suspensos na amostra, como partículas de solo e matéria orgânica. Essa medição não é equivalente à turbidez.",
    usage: "Complementa a caracterização da coleta e as entradas disponíveis para a estimativa de metais.",
  },
  carbono_organico_total: {
    symbol: "TOC",
    icon: LuLeaf,
    summary: "A presença de carbono em compostos orgânicos.",
    description: "Expressa a quantidade de carbono associada à matéria orgânica da amostra. Não identifica, por si só, a origem ou a composição de cada substância.",
    usage: "Complementa a caracterização da coleta e as entradas disponíveis para a estimativa de metais.",
  },
  fosforo_total: {
    symbol: "TP",
    icon: LuAtom,
    summary: "Um nutriente importante para o equilíbrio da água.",
    description: "Reúne as formas de fósforo quantificadas na amostra. Em excesso, esse nutriente pode favorecer o crescimento de algas e alterar o equilíbrio do ambiente aquático.",
    usage: "Complementa a caracterização da coleta. Confira a unidade antes de cadastrar o resultado.",
  },
};

// Nomes, obrigatoriedade e unidades acompanham o mesmo catálogo do formulário.
export const methodologyIndicators = indicatorFields.map((field) => ({
  ...field,
  ...indicatorExplanations[field.name],
}));

export const methodologyQuestions = [
  {
    question: "Preciso informar todos os parâmetros e metais?",
    answer: "Temperatura, pH, condutividade elétrica e oxigênio dissolvido são obrigatórios no cadastro. Os demais indicadores e os metais medidos são opcionais. Informe apenas resultados que você possui; um campo ausente não significa concentração zero.",
  },
  {
    question: "Posso salvar uma coleta sem usar a IA?",
    answer: "Sim. A opção de salvar coleta registra as medições sem executar a análise com IA. O nível de risco fica sem análise até que exista um resultado associado. Para analisar com IA, é necessária uma imagem: você pode anexá-la ou usar uma imagem já salva no corpo hídrico.",
  },
  {
    question: "A fotografia mede a concentração de metais?",
    answer: "Não. A visão computacional identifica sinais visuais na imagem. As estimativas de concentração vêm de modelos que usam os parâmetros da água. Um valor medido deve vir da medição informada na coleta; a aparência da água não confirma sua composição química.",
  },
  {
    question: "O nível de risco é o mesmo que o IQA?",
    answer: "Não. O risco do WaterPath é uma triagem em três níveis, acompanhada dos motivos avaliados. O Índice de Qualidade das Águas (IQA) possui uma metodologia própria, com nove parâmetros e pesos. O WaterPath não calcula esse índice a partir dos sete indicadores do cadastro nem o substitui pelo nível de risco.",
    reference: { label: "Conheça a metodologia do IQA na ANA", href: "https://www.ana.gov.br/portalpnqa/indicadores-indice-aguas.aspx" },
  },
  {
    question: "Um risco baixo significa que a água pode ser consumida?",
    answer: "Não. O resultado se limita aos dados e critérios avaliados e não certifica potabilidade, balneabilidade ou conformidade ambiental. A avaliação para um uso específico exige os ensaios e a interpretação técnica correspondentes.",
  },
];
