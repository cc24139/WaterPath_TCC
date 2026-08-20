import type { IconType } from "react-icons";
import {
  LuClipboardList,
  LuDroplet,
  LuGlobe,
  LuLockKeyhole,
  LuMapPin,
  LuRuler,
} from "react-icons/lu";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";
import type { AddWaterBodieFormState } from "@/features/add-water-bodie/types/addWaterBodie";

interface WaterBodySummaryProps {
  form: AddWaterBodieFormState;
}

interface SummaryItem {
  label: string;
  value: string;
  icon: IconType;
}

export function WaterBodySummary({ form }: WaterBodySummaryProps) {
  const items: SummaryItem[] = [
    { label: "Nome do corpo hídrico", value: form.name, icon: LuDroplet },
    { label: "Localização", value: form.location, icon: LuMapPin },
    {
      label: "Tamanho",
      value: formatWaterBodySize(form.size),
      icon: LuRuler,
    },
    {
      label: "Acesso",
      value: form.ehPrivado ? "Área privada" : "Público",
      icon: form.ehPrivado ? LuLockKeyhole : LuGlobe,
    },
  ];

  return (
    <Card className="!rounded-lg !px-5 !py-5 lg:!px-6">
      <CardHeader className="mb-6 flex items-start gap-4">
        <LuClipboardList className="mt-0.5 h-6 w-6 shrink-0 text-primary" />
        <span>
          <CardTitle className="text-[18px] sm:text-[19px]">
            Resumo do cadastro
          </CardTitle>
          <p className="mt-2 font-heading text-[12px] font-medium leading-relaxed text-text-secondary">
            Confira os dados informados até o momento.
          </p>
        </span>
      </CardHeader>

      <CardContent>
        <dl className="flex flex-col gap-4">
          {items.map((item) => (
            <div
              key={item.label}
              className="grid grid-cols-[22px_minmax(0,1fr)_minmax(24px,120px)] items-start gap-3"
            >
              <item.icon className="mt-0.5 h-4.5 w-4.5 text-text-secondary" />
              <dt className="font-heading text-[11px] font-semibold leading-relaxed text-text-primary sm:text-[12px]">
                {item.label}
              </dt>
              <dd className="break-words text-right font-heading text-[11px] font-semibold leading-relaxed text-text-secondary sm:text-[12px]">
                {item.value || "-"}
              </dd>
            </div>
          ))}
        </dl>
      </CardContent>
    </Card>
  );
}

function formatWaterBodySize(size: string) {
  if (!size.trim()) {
    return "";
  }

  const parsedSize = Number(size.replace(",", "."));

  if (!Number.isFinite(parsedSize)) {
    return size;
  }

  return `${new Intl.NumberFormat("pt-BR", {
    maximumFractionDigits: 2,
  }).format(parsedSize)} km`;
}
