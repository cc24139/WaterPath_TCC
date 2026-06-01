"use client";

import { SideBar } from "@/components/layout/SideBar";
import { ReportActions } from "@/features/report/components/ReportActions";
import { ReportPreviewPlaceholder } from "@/features/report/components/ReportPreviewPlaceholder";
import { ReportSettingsPanel } from "@/features/report/components/ReportSettingsPanel";
import { useReportConfig } from "@/features/report/hooks/useReportConfig";

export default function Report() {
  const {
    config,
    resetConfig,
    updateForecastPeriod,
    updateDetailLevel,
    updateDocumentLength,
    updateIncludedSection,
    updateAdditionalSetting,
    generateReport,
  } = useReportConfig();

  return (
    <div className="min-h-screen bg-background lg:flex">
      <SideBar />

      <main className="min-w-0 flex-1 px-4 py-5 sm:px-6 lg:px-8 xl:px-10">
        <div className="mx-auto flex w-full max-w-[1440px] flex-col gap-7">
          <header className="min-w-0">
            <h1 className="font-heading text-[26px] font-bold text-text-primary sm:text-[30px]">
              Relatório
            </h1>
            <p className="mt-1 font-heading text-[11px] font-medium text-[#767676] sm:text-[12px]">
              Configure seu relatório, visualize o preview e exporte em PDF.
            </p>
          </header>

          <section className="grid w-full min-w-0 grid-cols-1 gap-6 xl:grid-cols-[minmax(430px,560px)_minmax(0,1fr)] xl:items-start">
            <ReportSettingsPanel
              config={config}
              onForecastPeriodChange={updateForecastPeriod}
              onDetailLevelChange={updateDetailLevel}
              onDocumentLengthChange={updateDocumentLength}
              onIncludedSectionChange={updateIncludedSection}
              onAdditionalSettingChange={updateAdditionalSetting}
            />

            <ReportPreviewPlaceholder />
          </section>

          <ReportActions onReset={resetConfig} onGenerate={generateReport} />
        </div>
      </main>
    </div>
  );
}
