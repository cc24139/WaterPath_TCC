import { LuCalendarDays, LuDroplet, LuFlaskConical, LuTriangleAlert } from "react-icons/lu";
import { isRiskLevel, riskLabels } from "@/features/analysis-risk/utils/analysisRisk";
import { RiverConditionCard } from "./RiverConditionCard";

export function RiverConditionCards({ riskLevel, iqa, metalCount, collectedAt }: {
  riskLevel?: number | null;
  iqa: number | null;
  metalCount?: number;
  collectedAt?: string;
}) {
  const conditions = [
    { title: "Risco geral", value: isRiskLevel(riskLevel) ? riskLabels[riskLevel] : "Sem análise",
      description: "Classificação da IA (1 a 3)", colorClass: isRiskLevel(riskLevel)
        ? riskLevel === 1 ? "text-success" : riskLevel === 2 ? "text-warning" : "text-contrast" : "text-text-secondary",
      icon: <LuTriangleAlert className="h-6 w-6" aria-hidden="true" /> },
    { title: "Qualidade da água", value: iqa === null ? "Sem IQA" : `${iqa.toLocaleString("pt-BR", { maximumFractionDigits: 1 })} / 100`,
      description: "IQA medido na coleta", colorClass: "text-primary",
      icon: <LuDroplet className="h-6 w-6" aria-hidden="true" /> },
    { title: "Metais pesados", value: metalCount === undefined ? "Indisponível" : `${metalCount} estimativas`,
      description: "Valores estimados pela IA", colorClass: "text-text-secondary",
      icon: <LuFlaskConical className="h-6 w-6" aria-hidden="true" /> },
    { title: "Coleta analisada", value: collectedAt ? new Date(collectedAt).toLocaleDateString("pt-BR", { timeZone: "America/Sao_Paulo" }) : "Sem coleta",
      description: "Data da medição", colorClass: "text-primary",
      icon: <LuCalendarDays className="h-6 w-6" aria-hidden="true" /> },
  ];

  return <section aria-label="Resumo da análise" className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-4 xl:gap-4">
    {conditions.map((condition) => <RiverConditionCard key={condition.title} {...condition} />)}
  </section>;
}
