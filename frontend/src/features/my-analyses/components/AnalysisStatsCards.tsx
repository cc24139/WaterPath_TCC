import type { IconType } from "react-icons";
import {
  LuCircleCheck,
  LuFilePenLine,
  LuFileText,
  LuLoaderCircle,
} from "react-icons/lu";

import { Card } from "@/components/ui/Card";
import type { AnalysisStats } from "@/features/my-analyses/types/myAnalyses";

interface AnalysisStatsCardsProps {
  stats: AnalysisStats;
}

interface StatCardItem {
  label: string;
  value: number;
  icon: IconType;
  iconClassName: string;
}

export function AnalysisStatsCards({ stats }: AnalysisStatsCardsProps) {
  const items: StatCardItem[] = [
    {
      label: "Total",
      value: stats.total,
      icon: LuFileText,
      iconClassName: "bg-primary/10 text-primary",
    },
    {
      label: "Processando",
      value: stats.processing,
      icon: LuLoaderCircle,
      iconClassName: "bg-secondary/15 text-primary",
    },
    {
      label: "Concluídas",
      value: stats.completed,
      icon: LuCircleCheck,
      iconClassName: "bg-success/15 text-success",
    },
    {
      label: "Rascunhos",
      value: stats.drafts,
      icon: LuFilePenLine,
      iconClassName: "bg-placeholder/45 text-text-secondary",
    },
  ];

  return (
    <section
      aria-label="Resumo das análises"
      className="grid grid-cols-2 gap-3 sm:gap-5 xl:grid-cols-4"
    >
      {items.map((item) => {
        const Icon = item.icon;

        return (
          <Card
            key={item.label}
            className="flex min-w-0 items-center gap-3 !rounded-lg !px-4 !py-4 sm:gap-4 sm:!px-5 sm:!py-5"
          >
            <span
              className={`inline-flex h-12 w-12 shrink-0 items-center justify-center rounded-full sm:h-14 sm:w-14 ${item.iconClassName}`}
            >
              <Icon
                className={`h-5 w-5 sm:h-6 sm:w-6 ${
                  item.label === "Processando" ? "animate-spin" : ""
                }`}
              />
            </span>
            <span className="min-w-0">
              <span className="block truncate font-heading text-[11px] font-semibold text-text-secondary sm:text-[12px]">
                {item.label}
              </span>
              <strong className="mt-0.5 block font-heading text-[22px] font-bold leading-none text-text-primary sm:text-[25px]">
                {item.value}
              </strong>
              <span className="mt-1 block font-heading text-[10px] font-medium text-text-secondary sm:text-[11px]">
                {item.value === 1 ? "análise" : "análises"}
              </span>
            </span>
          </Card>
        );
      })}
    </section>
  );
}
