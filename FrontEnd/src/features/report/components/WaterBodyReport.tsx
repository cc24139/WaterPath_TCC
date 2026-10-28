"use client";

import { LuFileCheck, LuInfo } from "react-icons/lu";
import { ReportActions } from "./ReportActions";
import { ReportPreview } from "./ReportPreview";
import { ReportSettingsPanel } from "./ReportSettingsPanel";
import { useReportConfig } from "../hooks/useReportConfig";

export default function Report({ waterBodyId }: { waterBodyId: number }) {
  const report = useReportConfig(waterBodyId);
  return <div className="flex w-full min-w-0 flex-col gap-6">
    <header className="flex flex-wrap items-start justify-between gap-3"><div><span className="text-xs font-bold uppercase tracking-widest text-primary">WaterPath · Documentos</span><h1 className="mt-2 font-heading text-3xl font-bold">Relatório de monitoramento</h1><p className="mt-2 max-w-2xl text-sm leading-relaxed text-text-secondary">Reúna os resultados deste corpo hídrico em um documento claro, com medições e estimativas identificadas.</p></div><span className="inline-flex items-center gap-2 rounded-full bg-primary/10 px-3 py-2 text-xs font-semibold text-primary"><LuFileCheck aria-hidden="true" />Corpo hídrico #{waterBodyId}</span></header>
    <div className="flex items-start gap-3 rounded-lg border border-primary/15 bg-primary/5 px-4 py-3 text-xs leading-relaxed text-text-secondary"><LuInfo aria-hidden="true" className="mt-0.5 h-4 w-4 shrink-0 text-primary" /><p>O documento usa as coletas e análises já salvas. A geração consolida os resultados disponíveis no período, conforme suas permissões.</p></div>
    <div className="grid min-w-0 grid-cols-1 items-start gap-5 xl:grid-cols-[minmax(320px,0.8fr)_minmax(0,1.2fr)]">
      <ReportSettingsPanel config={report.config} onChange={report.setConfig} disabled={report.loading} />
      <div className="min-w-0 space-y-4 xl:sticky xl:top-5">
        {report.error && <p role="alert" className="rounded-lg border border-contrast/30 bg-white p-4 text-sm">{report.error}</p>}
        {report.validationError && <p role="status" className="rounded-lg bg-warning/10 p-4 text-sm">{report.validationError}</p>}
        <ReportPreview url={report.url} filename={report.filename} loading={report.loading} stale={report.stale} />
      </div>
    </div>
    <ReportActions onReset={report.resetConfig} onGenerate={report.generateReport} loading={report.loading} disabled={!!report.validationError || !report.isAuthenticated} />
  </div>;
}
