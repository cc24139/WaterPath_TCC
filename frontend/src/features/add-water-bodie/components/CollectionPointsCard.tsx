import { LuMapPin, LuPencil, LuPlus, LuTrash2, LuWaves } from "react-icons/lu";

import { Button } from "@/components/ui/Button";
import { FormSectionCard } from "@/components/ui/FormSectionCard";
import type { CollectionPoint } from "@/features/add-water-bodie/types/addWaterBodie";

interface CollectionPointsCardProps {
  points: CollectionPoint[];
  onAddPoint: () => void;
  onEditPoint: (point: CollectionPoint) => void;
  onRemovePoint: (pointId: number) => void;
}

export function CollectionPointsCard({
  points,
  onAddPoint,
  onEditPoint,
  onRemovePoint,
}: CollectionPointsCardProps) {
  return (
    <FormSectionCard
      title="Pontos de coleta"
      description="Cadastre os pontos onde serão realizadas as coletas de amostras."
      icon={LuWaves}
      headerAction={
        <Button
          variant="outline"
          onClick={onAddPoint}
          className="h-10 w-full border-primary px-4 text-primary sm:w-auto"
        >
          <LuPlus className="h-4 w-4" />
          Adicionar ponto
        </Button>
      }
    >
      {points.length ? (
        <ol className="flex flex-col gap-3">
          {points.map((point, index) => (
            <li
              key={point.id}
              className="grid gap-3 rounded-lg border border-placeholder px-3 py-3 sm:grid-cols-[32px_minmax(0,1fr)_100px_auto] sm:items-center sm:px-4 lg:grid-cols-[32px_minmax(0,1fr)_110px_auto]"
            >
              <span className="inline-flex h-7 w-7 items-center justify-center rounded-full bg-primary font-heading text-[11px] font-bold text-white">
                {index + 1}
              </span>

              <span className="min-w-0">
                <strong className="block font-heading text-[12px] font-bold text-text-primary">
                  {point.name}
                </strong>
                <span className="mt-1 flex items-start gap-1 font-heading text-[10px] font-medium leading-relaxed text-text-secondary sm:text-[11px]">
                  <LuMapPin className="mt-0.5 h-3 w-3 shrink-0" />
                  {point.reference}
                </span>
                {point.coordinates ? (
                  <span className="mt-0.5 block font-heading text-[10px] font-medium text-text-secondary sm:text-[11px]">
                    {point.coordinates}
                  </span>
                ) : null}
              </span>

              <span
                className={`w-fit rounded-md px-3 py-1.5 font-heading text-[10px] font-bold ${getPointBadgeClassName(
                  point.type
                )}`}
              >
                {point.type}
              </span>

              <span className="flex items-center gap-1 border-placeholder sm:justify-self-end sm:border-l sm:pl-3">
                <button
                  type="button"
                  onClick={() => onEditPoint(point)}
                  aria-label={`Editar ${point.name}`}
                  className="inline-flex h-9 w-9 items-center justify-center rounded-md text-text-secondary transition-colors hover:bg-primary/5 hover:text-primary focus:outline-none focus:ring-2 focus:ring-primary/30"
                >
                  <LuPencil className="h-4 w-4" />
                </button>
                <button
                  type="button"
                  onClick={() => onRemovePoint(point.id)}
                  aria-label={`Excluir ${point.name}`}
                  className="inline-flex h-9 w-9 items-center justify-center rounded-md text-text-secondary transition-colors hover:bg-contrast/10 hover:text-contrast focus:outline-none focus:ring-2 focus:ring-contrast/30"
                >
                  <LuTrash2 className="h-4 w-4" />
                </button>
              </span>
            </li>
          ))}
        </ol>
      ) : (
        <div className="rounded-lg border border-dashed border-placeholder px-4 py-8 text-center">
          <p className="font-heading text-[12px] font-semibold text-text-primary">
            Nenhum ponto de coleta cadastrado.
          </p>
          <p className="mt-1 font-heading text-[11px] text-text-secondary">
            Use “Adicionar ponto” para incluir o primeiro local de coleta.
          </p>
        </div>
      )}
    </FormSectionCard>
  );
}

function getPointBadgeClassName(type: CollectionPoint["type"]) {
  switch (type) {
    case "Montante":
      return "bg-secondary/20 text-primary";
    case "Jusante":
      return "bg-primary/10 text-primary";
    default:
      return "bg-placeholder/60 text-text-primary";
  }
}
