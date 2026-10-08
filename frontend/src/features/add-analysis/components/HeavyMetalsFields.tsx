import { FormField } from "@/components/ui/FormField";
import { metalOptions } from "../constants/addAnalysisOptions";
import type { AddAnalysisErrors, AddAnalysisFormState, MetalSymbol } from "../types/addAnalysis";

interface Props {
  metals: AddAnalysisFormState["metals"];
  errors: AddAnalysisErrors;
  onToggle: (symbol: MetalSymbol) => void;
  onChange: (symbol: MetalSymbol, value: string) => void;
}

export function HeavyMetalsFields({ metals, errors, onToggle, onChange }: Props) {
  const selected = metalOptions.filter(({ symbol }) => metals[symbol] !== undefined);
  return (
    <div className="space-y-5">
      <fieldset>
        <legend className="mb-3 font-heading text-xs font-bold text-text-primary">Selecione os metais medidos</legend>
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
          {metalOptions.map(({ symbol, label }) => (
            <label key={symbol} className={`flex cursor-pointer items-center gap-2 rounded-md border px-3 py-3 font-heading text-xs transition-colors ${metals[symbol] !== undefined ? "border-primary bg-primary/5 text-primary" : "border-placeholder bg-white text-text-secondary"}`}>
              <input type="checkbox" checked={metals[symbol] !== undefined} onChange={() => onToggle(symbol)} className="h-4 w-4 accent-primary focus-visible:outline-primary" />
              <span>{label} <span className="font-bold">({symbol})</span></span>
            </label>
          ))}
        </div>
      </fieldset>
      {selected.length ? (
        <div className="grid gap-x-6 gap-y-5 sm:grid-cols-2 xl:grid-cols-3">
          {selected.map(({ symbol, label, unit }) => (
            <FormField key={symbol} id={`add-analysis-${symbol}`} name={symbol} label={`${label} (${unit}) *`}
              value={metals[symbol]!} onChange={(value) => onChange(symbol, value)} required inputMode="decimal"
              placeholder="Informe a concentração" error={errors[symbol]} />
          ))}
        </div>
      ) : <p className="rounded-md bg-background px-4 py-3 font-heading text-xs text-text-secondary" role="status">Nenhuma medição de metais informada.</p>}
      <p className="font-heading text-[11px] leading-relaxed text-text-secondary">Metais não informados permanecem sem medição. As estimativas da IA serão apresentadas separadamente.</p>
    </div>
  );
}
