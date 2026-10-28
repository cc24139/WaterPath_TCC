import { additionalSettingOptions, detailLevelOptions, documentLengthOptions, includedSectionOptions } from "../constants/reportOptions";
import type { ReportConfig } from "../types/reportConfig";
import { CheckboxList } from "./CheckboxList";
import { RadioOptionGroup } from "./RadioOptionGroup";
import { ReportOptionCard } from "./ReportOptionCard";
import { DateRangeField } from "@/components/ui/date/DateRangeField";

export function ReportSettingsPanel({ config, onChange, disabled }: { config: ReportConfig; onChange: (config: ReportConfig) => void; disabled: boolean }) {
  return <fieldset disabled={disabled} className="min-w-0 space-y-4">
    <legend className="sr-only">Configuração do relatório</legend>
    <ReportOptionCard number={1} title="Período analisado" description="Datas de Brasília. Até 366 dias e 200 coletas por documento.">
      <DateRangeField id="report-period" label="Período das coletas" value={{ from: config.startDate, to: config.endDate }} disabled={disabled}
        onChange={({ from, to }) => onChange({ ...config, startDate: from, endDate: to })} />
    </ReportOptionCard>
    <ReportOptionCard number={2} title="Nível de detalhamento" description="O modo técnico acrescenta método e rastreabilidade. A classificação e os valores são preservados em todos os modos.">
      <RadioOptionGroup name="report-detail" options={detailLevelOptions} value={config.detailLevel} onChange={detailLevel => onChange({ ...config, detailLevel })} />
    </ReportOptionCard>
    <ReportOptionCard number={3} title="Extensão do documento" description="Compacto: até 5 linhas de histórico e 1 par de imagens. Padrão: até 30 linhas e 2 pares. Completo: todo o histórico e até 3 pares. A paginação acompanha o conteúdo; os motivos de atenção são sempre mantidos.">
      <RadioOptionGroup name="report-length" options={documentLengthOptions} value={config.documentLength} onChange={documentLength => onChange({ ...config, documentLength })} />
    </ReportOptionCard>
    <ReportOptionCard number={4} title="Seções incluídas" description="Identificação, abrangência e limites de interpretação são obrigatórios.">
      <CheckboxList options={includedSectionOptions} values={config.includedSections} onChange={(key, checked) => onChange({ ...config, includedSections: { ...config.includedSections, [key]: checked } })} />
    </ReportOptionCard>
    <ReportOptionCard number={5} title="Apresentação" description="Os destaques e as cores mudam a apresentação do documento. Os dados e a classificação de risco permanecem os mesmos.">
      <CheckboxList options={additionalSettingOptions} values={config.additionalSettings} onChange={(key, checked) => onChange({ ...config, additionalSettings: { ...config.additionalSettings, [key]: checked } })} />
    </ReportOptionCard>
    <div className="rounded-xl border border-dashed border-placeholder px-4 py-4 text-xs leading-relaxed text-text-secondary"><strong className="block text-text-primary">Próximas etapas</strong>Previsões de 7/15 dias, recomendações e tabela de contramedidas serão acrescentadas após a definição dos critérios. O relatório atual consolida resultados já disponíveis.</div>
  </fieldset>;
}
