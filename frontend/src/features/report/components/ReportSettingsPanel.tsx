import { additionalSettingOptions, detailLevelOptions, documentLengthOptions, includedSectionOptions } from "../constants/reportOptions";
import type { ReportConfig } from "../types/reportConfig";
import { CheckboxList } from "./CheckboxList";
import { RadioOptionGroup } from "./RadioOptionGroup";
import { ReportOptionCard } from "./ReportOptionCard";

export function ReportSettingsPanel({ config, onChange, disabled }: { config: ReportConfig; onChange: (config: ReportConfig) => void; disabled: boolean }) {
  const inputClass = "mt-2 h-11 w-full min-w-0 rounded-md border border-placeholder bg-white px-3 text-sm focus:outline-primary disabled:opacity-60";
  return <fieldset disabled={disabled} className="min-w-0 space-y-4">
    <legend className="sr-only">Configuração do relatório</legend>
    <ReportOptionCard number={1} title="Período analisado" description="Datas de Brasília. Até 366 dias e 200 coletas por documento.">
      <div className="grid grid-cols-1 gap-3 min-[400px]:grid-cols-2">
        <label className="min-w-0 text-xs font-semibold" htmlFor="report-start">De<input id="report-start" type="date" min="1900-01-01" max={config.endDate || undefined} value={config.startDate} onChange={e => onChange({ ...config, startDate: e.target.value })} className={inputClass} /></label>
        <label className="min-w-0 text-xs font-semibold" htmlFor="report-end">Até<input id="report-end" type="date" min={config.startDate || "1900-01-01"} value={config.endDate} onChange={e => onChange({ ...config, endDate: e.target.value })} className={inputClass} /></label>
      </div>
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
