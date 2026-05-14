import {
  LuCalendar,
  LuClock3,
  LuFilter,
  LuTriangleAlert,
} from "react-icons/lu";

import { RiverInsightListCard } from "./RiverInsightListCard";

const detectedProblems = [
  {
    title: "Alta Turbidez",
    description: "Valores acima do ideal podem comprometer a vida aquática.",
    colorClass: "text-[#FF4D4D]",
    icon: <LuTriangleAlert className="h-5 w-5" />,
  },
  {
    title: "DQO elevada",
    description: "Indica presença de matéria orgânica e possíveis poluentes.",
    colorClass: "text-[#FF4D4D]",
    icon: <LuTriangleAlert className="h-5 w-5" />,
  },
  {
    title: "Possível contaminação orgânica",
    description: "Detectada nos pontos próximos à ponte.",
    colorClass: "text-[#FFB020]",
    icon: <LuTriangleAlert className="h-5 w-5" />,
  },
];

const recommendations = [
  {
    title: "Investigar fontes de poluição",
    description: "Verificar possíveis lançamentos irregulares próximos aos pontos críticos.",
    colorClass: "text-primary",
    icon: <LuClock3 className="h-5 w-5" />,
  },
  {
    title: "Aumentar frequência de coleta",
    description: "Recomenda-se coleta diária nos próximos 7 dias.",
    colorClass: "text-primary",
    icon: <LuCalendar className="h-5 w-5" />,
  },
  {
    title: "Ações de mitigação",
    description: "Avaliar ações de limpeza e conscientização na região.",
    colorClass: "text-primary",
    icon: <LuFilter className="h-5 w-5" />,
  },
];

export function RiverInsightCards() {
  return (
    <section
      className="
        grid grid-cols-1 gap-4 px-4
        md:grid-cols-2
        xl:max-w-[760px] xl:grid-cols-1
      "
    >
      <RiverInsightListCard
        title="Problemas detectados"
        items={detectedProblems}
        actionLabel="Ver detalhes"
        actionHref="/analysis"
      />

      <RiverInsightListCard
        title="Recomendações"
        items={recommendations}
        actionLabel="Ver todas recomendações"
        actionHref="/report"
      />
    </section>
  );
}
