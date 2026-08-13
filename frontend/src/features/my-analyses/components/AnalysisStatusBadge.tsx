import type { IconType } from "react-icons";
import {
  LuCircleAlert,
  LuCircleCheck,
  LuFilePenLine,
  LuLoaderCircle,
} from "react-icons/lu";

import type { AnalysisStatus } from "@/features/my-analyses/types/myAnalyses";

interface AnalysisStatusBadgeProps {
  status: AnalysisStatus;
}

const statusConfig: Record<
  AnalysisStatus,
  { icon: IconType; className: string }
> = {
  Processando: {
    icon: LuLoaderCircle,
    className: "bg-primary/10 text-primary",
  },
  Concluída: {
    icon: LuCircleCheck,
    className: "bg-success/15 text-success",
  },
  Atenção: {
    icon: LuCircleAlert,
    className: "bg-warning/15 text-warning",
  },
  Rascunho: {
    icon: LuFilePenLine,
    className: "bg-placeholder/45 text-text-secondary",
  },
};

export function AnalysisStatusBadge({ status }: AnalysisStatusBadgeProps) {
  const config = statusConfig[status];
  const Icon = config.icon;

  return (
    <span
      className={`inline-flex min-h-8 w-fit items-center gap-2 rounded-lg px-3 py-1.5 font-heading text-[11px] font-bold ${config.className}`}
    >
      <Icon
        className={`h-4 w-4 ${status === "Processando" ? "animate-spin" : ""}`}
      />
      {status}
    </span>
  );
}
