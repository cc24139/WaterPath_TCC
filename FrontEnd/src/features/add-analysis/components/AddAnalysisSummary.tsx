import { LuClipboardList } from "react-icons/lu";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";
import { indicatorFields } from "../constants/addAnalysisOptions";
import { parseDecimal } from "../utils/analysisForm";
import type { AddAnalysisFormState } from "../types/addAnalysis";

interface Props {
  form: AddAnalysisFormState;
  waterBodyLabel: string;
  responsible: string;
  imageName: string;
  savedCollectionId: number | null;
}

export function AddAnalysisSummary({ form, waterBodyLabel, responsible, imageName, savedCollectionId }: Props) {
  const requiredCount = indicatorFields.filter((field) => field.required && parseDecimal(form[field.name]) !== null).length;
  const optionalCount = indicatorFields.filter((field) => !field.required && parseDecimal(form[field.name]) !== null).length;
  const date = form.analysisDate ? new Date(form.analysisDate) : null;
  const items = [
    ["Corpo hídrico", waterBodyLabel || "Não selecionado"],
    ["Data e hora da coleta", date && Number.isFinite(date.getTime()) ? date.toLocaleString("pt-BR", { dateStyle: "short", timeStyle: "short" }) : "Não informadas"],
    ["Responsável", responsible || "Não identificado"],
    ["Indicadores obrigatórios", `${requiredCount} de 4 preenchidos`],
    ["Complementares", `${optionalCount} de 3 preenchidos`],
    ["Metais medidos", `${Object.values(form.metals).filter((value) => parseDecimal(value!) !== null).length} de ${Object.keys(form.metals).length} selecionados preenchidos`],
    ["Imagem", imageName || "Não anexada"],
    ["Coleta", savedCollectionId ? `#${savedCollectionId} salva` : "Ainda não salva"],
  ];
  return (
    <Card className="!rounded-lg !px-5 !py-5 lg:!px-6">
      <CardHeader className="mb-6 flex items-start gap-4">
        <LuClipboardList className="mt-0.5 h-6 w-6 shrink-0 text-primary" />
        <span><CardTitle className="text-[18px] sm:text-[19px]">Resumo do envio</CardTitle>
          <p className="mt-2 font-heading text-xs text-text-secondary">Confira os dados da coleta.</p></span>
      </CardHeader>
      <CardContent>
        <dl className="flex flex-col gap-4 font-heading text-xs">
          {items.map(([label, value]) => (
            <div key={label} className="grid grid-cols-2 gap-3"><dt className="font-bold text-text-primary">{label}</dt><dd className="break-words text-right text-text-secondary">{value}</dd></div>
          ))}
        </dl>
      </CardContent>
    </Card>
  );
}
