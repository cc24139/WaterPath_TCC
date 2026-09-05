import Link from "next/link";
import { LuArrowRight, LuWaves } from "react-icons/lu";
import { Card } from "@/components/ui/Card";
import type { River } from "../types/river";
import { formatCollectionDate, formatMeasurement, getRiverStatusClassName } from "../utils/riverData";
import { RiverMeasurementsChart } from "./RiverMeasurementsChart";

interface RiverListProps {
  rivers: River[];
  selectedRiverId?: string;
  className?: string;
}

export function RiverList({ rivers, selectedRiverId, className = "" }: RiverListProps) {
  return (
    <section aria-label="Corpos hídricos" tabIndex={0} className={`flex w-full flex-col gap-5 rounded-lg focus-visible:outline-2 focus-visible:outline-primary sm:gap-6 lg:gap-7 ${className}`}>
      {rivers.map((river) => <RiverCard key={river.id} river={river} isSelected={river.id === selectedRiverId} />)}
    </section>
  );
}

function RiverCard({ river, isSelected }: { river: River; isSelected: boolean }) {
  const latest = river.measurements.at(-1);

  return (
    <Card className={`w-full shrink-0 border-2 !rounded-lg !px-3 !py-3 transition-colors sm:!rounded-2xl sm:!px-5 sm:!py-5 ${isSelected ? "border-primary" : "border-transparent"}`}>
      <article className="grid gap-4 md:grid-cols-[minmax(150px,190px)_minmax(0,1fr)] md:gap-5">
        <section className="min-w-0">
          <h2 className="break-words font-heading text-lg font-bold text-text-primary sm:text-xl">{river.name}</h2>
          <p className="mt-1 break-words text-xs text-text-secondary">{river.location}</p>
          <div className="mt-3 flex aspect-[16/9] flex-col items-center justify-center gap-2 rounded-lg bg-primary/5 text-text-secondary">
            <LuWaves aria-hidden="true" className="h-9 w-9 text-primary" />
            <span className="text-xs">Sem imagem disponível</span>
          </div>
          <p className="mt-3 text-xs text-text-secondary">{river.measurements.length} coleta(s)</p>
        </section>
        <section className="min-w-0 border-placeholder md:border-l md:pl-5">
          <RiverMeasurementsChart measurements={river.measurements} />
        </section>
        <section className="min-w-0 border-t border-placeholder pt-4 md:col-span-2">
          <h3 className="font-heading text-sm font-semibold">Última coleta</h3>
          <p className="mt-1 text-xs text-text-secondary">{latest ? `${formatCollectionDate(latest.timestamp)} (UTC)` : "Nenhuma coleta disponível."}</p>
          <dl className="mt-3 grid grid-cols-1 gap-2 text-xs text-text-secondary sm:grid-cols-3">
            <InfoItem label="pH" value={formatMeasurement(latest?.ph)} />
            <InfoItem label="Turbidez" value={formatMeasurement(latest?.turbidity)} />
            <InfoItem label="Oxigênio dissolvido" value={formatMeasurement(latest?.dissolvedOxygen)} />
          </dl>
          <div className="mt-4 flex items-center justify-between gap-4 border-t border-placeholder/60 pt-3">
            <div className="min-w-0 flex-1">
              <h3 className="font-heading text-sm font-semibold">IQA</h3>
            {river.iqa !== null ? (
              <>
                <div className="mt-2 flex flex-wrap items-center gap-3">
                  <span className="text-sm font-bold">{formatMeasurement(river.iqa)} / 100</span>
                  <span className={`rounded-full px-3 py-1 text-xs font-semibold ${getRiverStatusClassName(river.status)}`}>{river.status}</span>
                </div>
                <p className="mt-2 text-xs text-text-secondary">Registro sem data; pode não corresponder à última coleta.</p>
              </>
            ) : (
              <p className="mt-2 text-xs text-text-secondary">{river.qualityRecordCount > 1 ? "Há vários registros sem data. Não é possível determinar o IQA mais recente." : "Sem IQA válido disponível."}</p>
            )}
            {river.qualityRecordCount > 1 && river.iqaValues.length > 0 && <p className="mt-2 break-words text-xs text-text-secondary">Valores registrados (escala de 0 a 100, sem ordem cronológica): {river.iqaValues.map(formatMeasurement).join("; ")}.</p>}
            </div>
            <Link
              href={`/water-bodies/${river.id}`}
              aria-label={`Ver análise completa de ${river.name}`}
              className="inline-flex shrink-0 items-center gap-2 rounded py-2 font-heading text-sm font-semibold text-primary transition-colors hover:text-secondary focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-primary"
            >
              Ver Análise
              <LuArrowRight aria-hidden="true" className="h-5 w-5" />
            </Link>
          </div>
        </section>
      </article>
    </Card>
  );
}

function InfoItem({ label, value }: { label: string; value: string }) {
  return <div><dt className="font-semibold">{label}</dt><dd className="mt-1">{value}</dd></div>;
}
