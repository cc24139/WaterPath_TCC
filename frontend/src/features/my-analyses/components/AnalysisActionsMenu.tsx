import type { MouseEvent } from "react";
import type { IconType } from "react-icons";
import {
  LuCopy,
  LuEllipsisVertical,
  LuEye,
  LuPencil,
  LuTrash2,
} from "react-icons/lu";

import type {
  AnalysisMenuAction,
  UserAnalysis,
} from "@/features/my-analyses/types/myAnalyses";

interface AnalysisActionsMenuProps {
  analysis: UserAnalysis;
  onAction: (action: AnalysisMenuAction, analysis: UserAnalysis) => void;
}

const menuItems: Array<{
  action: AnalysisMenuAction;
  label: string;
  icon: IconType;
}> = [
  { action: "details", label: "Ver detalhes", icon: LuEye },
  { action: "edit", label: "Editar", icon: LuPencil },
  { action: "duplicate", label: "Duplicar análise", icon: LuCopy },
  { action: "delete", label: "Excluir", icon: LuTrash2 },
];

export function AnalysisActionsMenu({
  analysis,
  onAction,
}: AnalysisActionsMenuProps) {
  function handleAction(
    event: MouseEvent<HTMLButtonElement>,
    action: AnalysisMenuAction
  ) {
    event.currentTarget.closest("details")?.removeAttribute("open");
    onAction(action, analysis);
  }

  return (
    <details className="group relative">
      <summary
        aria-label={`Mais ações para ${analysis.waterBodyName}`}
        className="inline-flex h-9 w-9 cursor-pointer list-none items-center justify-center rounded-md text-text-secondary transition-colors hover:bg-primary/5 hover:text-primary focus:outline-none focus:ring-2 focus:ring-primary/30 [&::-webkit-details-marker]:hidden"
      >
        <LuEllipsisVertical className="h-5 w-5" />
      </summary>

      <div className="absolute right-0 top-10 z-30 w-44 rounded-lg border border-placeholder bg-white p-1.5 shadow-default">
        {menuItems.map((item) => {
          const Icon = item.icon;

          return (
            <button
              key={item.action}
              type="button"
              onClick={(event) => handleAction(event, item.action)}
              className={`flex min-h-9 w-full items-center gap-2 rounded-md px-3 text-left font-heading text-[11px] font-semibold transition-colors ${
                item.action === "delete"
                  ? "text-contrast hover:bg-contrast/10"
                  : "text-text-primary hover:bg-primary/5 hover:text-primary"
              }`}
            >
              <Icon className="h-4 w-4 shrink-0" />
              {item.label}
            </button>
          );
        })}
      </div>
    </details>
  );
}
