import { LuSearch } from "react-icons/lu";

import { Card } from "@/components/ui/Card";
import { FormField } from "@/components/ui/FormField";
import {
  sortOptions,
  statusFilterOptions,
} from "@/features/my-analyses/constants/mockUserAnalyses";
import type {
  AnalysisSortOption,
  AnalysisStatusFilter,
} from "@/features/my-analyses/types/myAnalyses";

interface AnalysisFiltersProps {
  search: string;
  statusFilter: AnalysisStatusFilter;
  sortBy: AnalysisSortOption;
  onSearchChange: (value: string) => void;
  onStatusChange: (value: AnalysisStatusFilter) => void;
  onSortChange: (value: AnalysisSortOption) => void;
}

export function AnalysisFilters({
  search,
  statusFilter,
  sortBy,
  onSearchChange,
  onStatusChange,
  onSortChange,
}: AnalysisFiltersProps) {
  return (
    <Card className="grid gap-5 !rounded-lg !px-5 !py-5 md:grid-cols-2 xl:grid-cols-[minmax(260px,1.3fr)_minmax(210px,1fr)_minmax(210px,1fr)] xl:items-end xl:gap-7 xl:!px-6">
      <div className="min-w-0 md:col-span-2 xl:col-span-1">
        <label htmlFor="analysis-search" className="sr-only">
          Buscar por corpo hídrico, cidade ou ponto de coleta
        </label>
        <span className="relative block">
          <LuSearch className="pointer-events-none absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-text-secondary" />
          <input
            id="analysis-search"
            type="search"
            value={search}
            onChange={(event) => onSearchChange(event.target.value)}
            placeholder="Buscar análises..."
            className="h-12 w-full rounded-md border border-placeholder bg-white pl-12 pr-4 font-heading text-[12px] font-medium text-text-primary outline-none transition-colors placeholder:text-text-secondary/75 focus:border-primary focus:ring-2 focus:ring-secondary/25"
          />
        </span>
      </div>

      <FormField
        id="analysis-status-filter"
        label="Filtrar por status"
        name="statusFilter"
        value={statusFilter}
        onChange={(value) => onStatusChange(value as AnalysisStatusFilter)}
        options={statusFilterOptions}
      />

      <FormField
        id="analysis-sort"
        label="Ordenar por"
        name="sortBy"
        value={sortBy}
        onChange={(value) => onSortChange(value as AnalysisSortOption)}
        options={sortOptions}
      />
    </Card>
  );
}
