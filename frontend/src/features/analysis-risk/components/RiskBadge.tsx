import { LuShieldCheck, LuShieldAlert, LuCircleHelp } from "react-icons/lu";
import { isRiskLevel, riskLabels } from "../utils/analysisRisk";

export function RiskBadge({ level, unavailable = false }: { level?: number | null; unavailable?: boolean }) {
  const valid = isRiskLevel(level);
  const Icon = !valid ? LuCircleHelp : level === 1 ? LuShieldCheck : LuShieldAlert;
  const color = !valid ? "bg-placeholder/40 text-text-secondary" : level === 1 ? "bg-success/15 text-success"
    : level === 2 ? "bg-warning/15 text-warning" : "bg-contrast/10 text-contrast";
  return <span className={`inline-flex min-h-8 w-fit items-center gap-2 rounded-lg px-3 py-1.5 font-heading text-[11px] font-bold ${color}`}>
    <Icon aria-hidden="true" className="h-4 w-4 shrink-0" />
    {valid ? `Risco ${riskLabels[level].toLowerCase()} · ${level}` : unavailable ? "Risco indisponível" : "Sem análise de risco"}
  </span>;
}
