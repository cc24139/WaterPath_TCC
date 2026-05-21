import {
  additionalSettingOptions,
  documentLengthOptions,
  forecastPeriodOptions,
  includedSectionOptions,
} from "../constants/reportOptions";
import type {
  DetailLevel,
  DocumentLength,
  ForecastPeriod,
  ReportConfig,
} from "../types/reportConfig";
import { CheckboxList } from "./CheckboxList";
import { DetailLevelSelector } from "./DetailLevelSelector";
import { RadioOptionGroup } from "./RadioOptionGroup";
import { ReportOptionCard } from "./ReportOptionCard";

interface ReportSettingsPanelProps {
  config: ReportConfig;
  onForecastPeriodChange: (forecastPeriod: ForecastPeriod) => void;
  onDetailLevelChange: (detailLevel: DetailLevel) => void;
  onDocumentLengthChange: (documentLength: DocumentLength) => void;
  onIncludedSectionChange: (
    key: keyof ReportConfig["includedSections"],
    checked: boolean,
  ) => void;
  onAdditionalSettingChange: (
    key: keyof ReportConfig["additionalSettings"],
    checked: boolean,
  ) => void;
}

export function ReportSettingsPanel({
  config,
  onForecastPeriodChange,
  onDetailLevelChange,
  onDocumentLengthChange,
  onIncludedSectionChange,
  onAdditionalSettingChange,
}: ReportSettingsPanelProps) {
  return (
    <div className="flex w-full min-w-0 flex-col gap-5 sm:gap-6">
      <ReportOptionCard number={1} title="Período de previsão">
        <RadioOptionGroup
          name="forecastPeriod"
          options={forecastPeriodOptions}
          value={config.forecastPeriod}
          onChange={onForecastPeriodChange}
        />
      </ReportOptionCard>

      <ReportOptionCard number={2} title="Nível de detalhamento">
        <DetailLevelSelector
          value={config.detailLevel}
          onChange={onDetailLevelChange}
        />
      </ReportOptionCard>

      <ReportOptionCard number={3} title="Extensão do documento">
        <RadioOptionGroup
          name="documentLength"
          options={documentLengthOptions}
          value={config.documentLength}
          onChange={onDocumentLengthChange}
        />
      </ReportOptionCard>

      <ReportOptionCard number={4} title="Seções incluídas">
        <CheckboxList
          options={includedSectionOptions}
          values={config.includedSections}
          onChange={onIncludedSectionChange}
        />
      </ReportOptionCard>

      <ReportOptionCard number={5} title="Configurações adicionais">
        <CheckboxList
          options={additionalSettingOptions}
          values={config.additionalSettings}
          onChange={onAdditionalSettingChange}
        />
      </ReportOptionCard>
    </div>
  );
}
