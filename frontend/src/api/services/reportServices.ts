import { apiFetch } from "../apiFetch";
import { routes } from "../routes";
import type { ReportConfig } from "@/features/report/types/reportConfig";

export const reportServices = {
  generate(waterBodyId: number, config: ReportConfig, signal: AbortSignal) {
    return apiFetch(routes.relatorios + "pdf", { method: "POST", headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ corpoHidricoId: waterBodyId, config }), signal }, { redirectOnUnauthorized: false });
  },
};
