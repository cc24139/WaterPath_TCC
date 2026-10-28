"use client";

import { useEffect, useRef, useState } from "react";
import { useAuthSession } from "@/features/auth/hooks/useAuthSession";
import { reportServices } from "@/api/services/reportServices";
import { createDefaultReportConfig } from "../constants/reportOptions";
import { reportConfigKey, validateReportConfig } from "../utils/reportConfig";
import { readReportPdf } from "../utils/reportResponse";

export function useReportConfig(waterBodyId: number) {
  const { user, isAuthenticated } = useAuthSession();
  const [config, setConfig] = useState(createDefaultReportConfig);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<{ url: string; key: string; owner: string; filename: string } | null>(null);
  const controller = useRef<AbortController | null>(null);
  const pending = useRef(false);
  useEffect(() => () => { controller.current?.abort(); pending.current = false; }, [user?.id]);
  useEffect(() => () => { if (result) URL.revokeObjectURL(result.url); }, [result]);
  const visible = result?.owner === user?.id ? result : null;
  const validationError = validateReportConfig(config);

  async function generateReport() {
    if (pending.current) return;
    const invalid = validateReportConfig(config);
    if (invalid || !user) { setError(invalid || "Entre novamente para gerar o relatório."); return; }
    const abort = new AbortController();
    controller.current = abort;
    pending.current = true; setLoading(true); setError(null);
    const key = reportConfigKey(config);
    let timedOut = false;
    const timeout = setTimeout(() => { timedOut = true; abort.abort(); }, 120000);
    try {
      const blob = await readReportPdf(await reportServices.generate(waterBodyId, config, abort.signal));
      if (!abort.signal.aborted) setResult({ url: URL.createObjectURL(blob), key, owner: user.id, filename: `waterpath-rio-${waterBodyId}-${config.endDate}.pdf` });
    } catch (failure) {
      if (timedOut) setError("A geração demorou além do esperado. Reduza o período e tente novamente.");
      else if (!abort.signal.aborted) setError(failure instanceof TypeError ? "Não foi possível conectar ao servidor. Tente novamente." : failure instanceof Error ? failure.message : "Não foi possível gerar o relatório.");
    } finally {
      clearTimeout(timeout);
      if (controller.current === abort) { pending.current = false; setLoading(false); }
    }
  }
  return { config, setConfig, error, loading, validationError, isAuthenticated,
    url: visible?.url ?? null, filename: visible?.filename ?? "relatorio.pdf", stale: !!visible && visible.key !== reportConfigKey(config),
    resetConfig: () => { setConfig(createDefaultReportConfig()); setError(null); }, generateReport };
}
