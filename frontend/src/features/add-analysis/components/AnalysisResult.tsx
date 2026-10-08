import { LuCircleCheck } from "react-icons/lu";
import type { PredicaoDTO } from "@/api/dtos/predicaoDTO";
import { AddAnalysisFormSection } from "./AddAnalysisFormSection";
import { metalOptions } from "../constants/addAnalysisOptions";

export function AnalysisResult({ prediction }: { prediction: PredicaoDTO }) {
  const result = prediction.resultado;
  return (
    <AddAnalysisFormSection title="Resultado da análise com IA" description={`Análise #${prediction.id} · Coleta #${prediction.coletaId}`} icon={LuCircleCheck}>
      <div className="space-y-4 font-heading text-xs">
        <p className="font-bold text-text-primary">Risco {result.riskLabel} · Nível {result.riskLevel}</p>
        {result.riskReasons.length > 0 && <ul className="list-disc space-y-1 pl-5 text-text-secondary">{result.riskReasons.map((reason, index) => <li key={index}>{reason}</li>)}</ul>}
        <div className="border-t border-placeholder pt-4">
          <h3 className="font-bold text-text-primary">Estimativas de metais da IA</h3>
          <p className="mt-1 text-text-secondary">Concentrações estimadas pelo modelo. As medições informadas permanecem no formulário acima.</p>
          <dl className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            {result.metalPredictions.map((metal) => (
              <div key={metal.name} className="rounded-md bg-background p-3">
                <dt className="text-text-secondary">{metalOptions.find(({ symbol }) => symbol === metal.name)?.label ?? metal.name} ({metal.name})</dt>
                <dd className="mt-1 font-bold text-text-primary">{metal.value.toLocaleString("pt-BR", { maximumSignificantDigits: 6 })} {metal.unit}</dd>
              </div>
            ))}
          </dl>
          {!result.metalPredictions.length && <p className="mt-3 text-text-secondary">Nenhuma estimativa de metal retornada.</p>}
        </div>
      </div>
    </AddAnalysisFormSection>
  );
}
