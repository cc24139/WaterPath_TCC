"use client";

import { useState } from "react";

import { defaultReportConfig } from "../constants/reportOptions";
import type {
  DetailLevel,
  DocumentLength,
  ForecastPeriod,
  ReportConfig,
} from "../types/reportConfig";

export function useReportConfig() {
  const [config, setConfig] = useState<ReportConfig>(defaultReportConfig);

  function resetConfig() {
    setConfig(defaultReportConfig);
  }

  function updateForecastPeriod(forecastPeriod: ForecastPeriod) {
    setConfig((currentConfig) => ({
      ...currentConfig,
      forecastPeriod,
    }));
  }

  function updateDetailLevel(detailLevel: DetailLevel) {
    setConfig((currentConfig) => ({
      ...currentConfig,
      detailLevel,
    }));
  }

  function updateDocumentLength(documentLength: DocumentLength) {
    setConfig((currentConfig) => ({
      ...currentConfig,
      documentLength,
    }));
  }

  function updateIncludedSection(
    key: keyof ReportConfig["includedSections"],
    checked: boolean,
  ) {
    setConfig((currentConfig) => ({
      ...currentConfig,
      includedSections: {
        ...currentConfig.includedSections,
        [key]: checked,
      },
    }));
  }

  function updateAdditionalSetting(
    key: keyof ReportConfig["additionalSettings"],
    checked: boolean,
  ) {
    setConfig((currentConfig) => ({
      ...currentConfig,
      additionalSettings: {
        ...currentConfig.additionalSettings,
        [key]: checked,
      },
    }));
  }

  function generateReport() {
    // Futuramente, conecte aqui a chamada para API de geração/preview do relatório.
  }

  return {
    config,
    setConfig,
    resetConfig,
    updateForecastPeriod,
    updateDetailLevel,
    updateDocumentLength,
    updateIncludedSection,
    updateAdditionalSetting,
    generateReport,
  };
}
