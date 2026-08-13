import { LuFileText, LuListFilter, LuSquarePlus } from "react-icons/lu";

import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";

interface EmptyAnalysesStateProps {
  isFiltered: boolean;
  onCreateAnalysis: () => void;
  onClearFilters: () => void;
}

export function EmptyAnalysesState({
  isFiltered,
  onCreateAnalysis,
  onClearFilters,
}: EmptyAnalysesStateProps) {
  return (
    <Card className="flex min-h-80 flex-col items-center justify-center !rounded-lg !px-6 !py-10 text-center">
      <span className="inline-flex h-16 w-16 items-center justify-center rounded-full bg-primary/10 text-primary">
        {isFiltered ? (
          <LuListFilter className="h-7 w-7" />
        ) : (
          <LuFileText className="h-7 w-7" />
        )}
      </span>
      <h2 className="mt-5 font-heading text-[19px] font-bold text-text-primary">
        {isFiltered
          ? "Nenhuma análise encontrada."
          : "Nenhuma análise cadastrada ainda."}
      </h2>
      <p className="mt-2 max-w-lg font-heading text-[12px] font-medium leading-relaxed text-text-secondary sm:text-[13px]">
        {isFiltered
          ? "Tente ajustar a busca ou os filtros para encontrar outra análise."
          : "Cadastre sua primeira análise para começar a acompanhar os dados de um corpo hídrico."}
      </p>
      <Button
        onClick={isFiltered ? onClearFilters : onCreateAnalysis}
        variant={isFiltered ? "outline" : "primary"}
        className="mt-6 min-w-44"
      >
        {isFiltered ? null : <LuSquarePlus className="h-4 w-4" />}
        {isFiltered ? "Limpar filtros" : "Criar nova análise"}
      </Button>
    </Card>
  );
}
