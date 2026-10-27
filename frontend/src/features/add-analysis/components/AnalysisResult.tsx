import { RiskBadge } from "@/features/analysis-risk/components/RiskBadge";
import { LuCircleCheck } from "react-icons/lu";
import type { PredicaoDTO } from "@/api/dtos/predicaoDTO";
import { AddAnalysisFormSection } from "./AddAnalysisFormSection";
import { metalOptions } from "../constants/addAnalysisOptions";

export function AnalysisResult({ prediction }: { prediction: PredicaoDTO }) {
  const result = prediction.resultado;
  const estimatedMetals = result.estimatedMetals ?? result.metalPredictions;
  return (
    <AddAnalysisFormSection title="Resultado da análise com IA" description={`Análise #${prediction.id} · Coleta #${prediction.coletaId}`} icon={LuCircleCheck}>
      <div className="space-y-4 font-heading text-xs">
        <RiskBadge level={result.riskLevel} unavailable />
        {result.riskReasons.length > 0 && <ul className="list-disc space-y-1 pl-5 text-text-secondary">{result.riskReasons.map((reason, index) => <li key={index}>{reason}</li>)}</ul>}
        <div className="border-t border-placeholder pt-4">
          <h3 className="font-bold text-text-primary">Metais da coleta</h3>
          <p className="mt-1 text-text-secondary">Valores medidos são preservados; o modelo estima os metais sem medição.</p>
          {!!result.measuredMetals?.length && <>
            <h4 className="mt-4 font-semibold text-text-primary">Medidos</h4>
            <dl className="mt-2 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
              {result.measuredMetals.map((metal) => <div key={metal.name} className="rounded-md bg-background p-3">
                <dt className="text-text-secondary">{metalOptions.find(({ symbol }) => symbol === metal.name)?.label ?? metal.name} ({metal.name})</dt>
                <dd className="mt-1 font-bold text-text-primary">{metal.value.toLocaleString("pt-BR", { maximumSignificantDigits: 6 })} {metal.unit}</dd>
              </div>)}
            </dl>
          </>}
          {!!estimatedMetals.length && <h4 className="mt-4 font-semibold text-text-primary">Estimados pela IA</h4>}
          <dl className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            {estimatedMetals.map((metal) => (
              <div key={metal.name} className="rounded-md bg-background p-3">
                <dt className="text-text-secondary">{metalOptions.find(({ symbol }) => symbol === metal.name)?.label ?? metal.name} ({metal.name})</dt>
                <dd className="mt-1 font-bold text-text-primary">{metal.value.toLocaleString("pt-BR", { maximumSignificantDigits: 6 })} {metal.unit}</dd>
              </div>
            ))}
          </dl>
          {!estimatedMetals.length && !result.measuredMetals?.length && <p className="mt-3 text-text-secondary">Nenhuma medição ou estimativa de metal retornada.</p>}
        </div>
      </div>
    </AddAnalysisFormSection>
  );
}
