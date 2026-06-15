import type { IconType } from "react-icons";
import {
  LuCalendar,
  LuClipboardList,
  LuFolder,
  LuImage,
  LuMapPin,
  LuUser,
} from "react-icons/lu";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";
import type { AddAnalysisFormState } from "@/features/add-analysis/types/addAnalysis";

interface AddAnalysisSummaryProps {
  form: AddAnalysisFormState;
}

interface SummaryItem {
  label: string;
  value: string;
  icon: IconType;
}

export function AddAnalysisSummary({ form }: AddAnalysisSummaryProps) {
  const summaryItems: SummaryItem[] = [
    {
      label: "Corpo hídrico",
      value: form.waterBody,
      icon: LuFolder,
    },
    {
      label: "Data da análise",
      value: formatDate(form.analysisDate),
      icon: LuCalendar,
    },
    {
      label: "Ponto de coleta",
      value: form.collectionPoint,
      icon: LuMapPin,
    },
    {
      label: "Responsável",
      value: form.responsible,
      icon: LuUser,
    },
    {
      label: "Tipo de análise",
      value: form.analysisType,
      icon: LuFolder,
    },
  ];

  return (
    <Card className="!rounded-lg !px-5 !py-5 lg:!px-6">
      <CardHeader className="mb-6 flex items-start gap-4">
        <LuClipboardList className="mt-0.5 h-6 w-6 shrink-0 text-primary" />
        <span>
          <CardTitle className="text-[18px] sm:text-[19px]">
            Resumo do envio
          </CardTitle>
          <p className="mt-2 font-heading text-[12px] font-medium leading-relaxed text-text-secondary">
            Confira os dados informados até o momento.
          </p>
        </span>
      </CardHeader>

      <CardContent>
        <dl className="flex flex-col gap-4">
          {summaryItems.map((item) => (
            <SummaryRow key={item.label} item={item} />
          ))}
        </dl>

        <div className="my-5 border-t border-dashed border-placeholder" />

        <SummaryRow
          item={{
            label: "Imagem anexada",
            value: form.imageName,
            icon: LuImage,
          }}
          description="Faça upload da imagem da análise para complementar o registro."
        />
      </CardContent>
    </Card>
  );
}

function SummaryRow({
  item,
  description,
}: {
  item: SummaryItem;
  description?: string;
}) {
  const Icon = item.icon;

  return (
    <div className="grid grid-cols-[24px_minmax(0,1fr)_auto] items-start gap-3">
      <Icon className="mt-0.5 h-5 w-5 text-text-secondary" />
      <span className="min-w-0">
        <dt className="font-heading text-[12px] font-bold text-text-primary">
          {item.label}
        </dt>
        {description ? (
          <dd className="mt-1 max-w-[230px] font-heading text-[11px] font-medium leading-relaxed text-text-secondary">
            {description}
          </dd>
        ) : null}
      </span>
      <dd className="max-w-[120px] truncate text-right font-heading text-[12px] font-semibold text-text-secondary">
        {item.value || "-"}
      </dd>
    </div>
  );
}

function formatDate(date: string) {
  if (!date) {
    return "";
  }

  const [year, month, day] = date.split("-");

  if (!year || !month || !day) {
    return date;
  }

  return `${day}/${month}/${year}`;
}
