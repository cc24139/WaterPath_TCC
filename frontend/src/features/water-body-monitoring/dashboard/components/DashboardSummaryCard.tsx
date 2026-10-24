import type { ReactNode } from "react";
import { DashboardPanel } from "./DashboardPanel";

export function DashboardSummaryCard({ title, value, detail, icon, action }: {
  title: string;
  value: ReactNode;
  detail: string;
  icon?: ReactNode;
  action?: { label: string; href: string };
}) {
  return <DashboardPanel title={title} action={action} className="h-full">
    <div className="flex min-h-11 items-center gap-3">
      {icon && <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-background text-primary">{icon}</span>}
      <div className="min-w-0 break-words font-heading text-sm font-semibold text-text-primary">{value}</div>
    </div>
    <p className="mt-3 text-xs leading-relaxed text-text-secondary">{detail}</p>
  </DashboardPanel>;
}
