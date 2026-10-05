import { LuTriangleAlert } from "react-icons/lu";
import { IoWaterOutline } from "react-icons/io5";
import { FaVial } from "react-icons/fa";
import { TbChartDots2 } from "react-icons/tb";

import { RiverConditionCard } from "./RiverConditionCard";

const riverConditions = [
  {
    title: "Risco Geral",
    value: "Alto",
    description: "Nível de risco atual",
    colorClass: "text-[#FF4D4D]",
    icon: <LuTriangleAlert className="text-[22px] sm:text-[26px] xl:text-[32px]" />,
  },
  {
    title: "Qualidade da Água",
    value: "Ruim",
    description: "Índice atual",
    colorClass: "text-primary",
    icon: <IoWaterOutline className="text-[22px] sm:text-[26px] xl:text-[32px]" />,
  },
  {
    title: "Metais Pesados",
    value: "Médio",
    description: "Concentração de metais",
    colorClass: "text-[#6B7280]",
    icon: <FaVial className="text-[20px] sm:text-[24px] xl:text-[28px]" />,
  },
  {
    title: "Expectativa Futura",
    value: "Piora",
    description: "Piora dos índices do rio",
    colorClass: "text-primary",
    icon: <TbChartDots2 className="text-[22px] sm:text-[26px] xl:text-[32px]" />,
  },
];

export function RiverConditionCards() {
  return (
    <section
      className="
        grid
        grid-cols-2
        xl:grid-cols-4
        gap-3
        sm:gap-4
        xl:gap-8
        px-4
      "
    >
      {riverConditions.map((condition) => (
        <RiverConditionCard
          key={condition.title}
          icon={condition.icon}
          title={condition.title}
          value={condition.value}
          description={condition.description}
          colorClass={condition.colorClass}
        />
      ))}
    </section>
  );
}
