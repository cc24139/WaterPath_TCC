import type { IconType } from "react-icons";
import {
  LuChartNoAxesColumnIncreasing,
  LuChevronLeft,
  LuChevronRight,
  LuEye,
  LuMapPin,
  LuPencil,
  LuWaves,
} from "react-icons/lu";

import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { AnalysisActionsMenu } from "@/features/my-analyses/components/AnalysisActionsMenu";
import { AnalysisStatusBadge } from "@/features/my-analyses/components/AnalysisStatusBadge";
import { EmptyAnalysesState } from "@/features/my-analyses/components/EmptyAnalysesState";
import type {
  AnalysisMenuAction,
  UserAnalysis,
} from "@/features/my-analyses/types/myAnalyses";

interface UserAnalysesListProps {
  analyses: UserAnalysis[];
  totalItems: number;
  totalAnalyses: number;
  currentPage: number;
  totalPages: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPrimaryAction: (analysis: UserAnalysis) => void;
  onMenuAction: (action: AnalysisMenuAction, analysis: UserAnalysis) => void;
  onCreateAnalysis: () => void;
  onClearFilters: () => void;
}

export function UserAnalysesList({
  analyses,
  totalItems,
  totalAnalyses,
  currentPage,
  totalPages,
  pageSize,
  onPageChange,
  onPrimaryAction,
  onMenuAction,
  onCreateAnalysis,
  onClearFilters,
}: UserAnalysesListProps) {
  if (!analyses.length) {
    return (
      <EmptyAnalysesState
        isFiltered={totalAnalyses > 0 && totalItems === 0}
        onCreateAnalysis={onCreateAnalysis}
        onClearFilters={onClearFilters}
      />
    );
  }

  return (
    <section aria-label="Análises cadastradas">
      <Card className="hidden !rounded-lg !p-0 xl:block">
        <table className="w-full table-fixed border-collapse text-left">
          <caption className="sr-only">
            Lista das análises cadastradas pelo usuário
          </caption>
          <colgroup>
            <col className="w-[25%]" />
            <col className="w-[21%]" />
            <col className="w-[17%]" />
            <col className="w-[17%]" />
            <col className="w-[20%]" />
          </colgroup>
          <thead>
            <tr className="border-b border-placeholder">
              {[
                "Corpo hídrico",
                "Ponto de coleta",
                "Data da análise",
                "Status",
                "Ações",
              ].map((heading) => (
                <th
                  key={heading}
                  scope="col"
                  className="px-4 py-5 font-heading text-[12px] font-bold text-text-primary first:pl-6 last:pr-6 xl:px-5"
                >
                  {heading}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {analyses.map((analysis) => (
              <AnalysisTableRow
                key={analysis.id}
                analysis={analysis}
                onPrimaryAction={onPrimaryAction}
                onMenuAction={onMenuAction}
              />
            ))}
          </tbody>
        </table>

        <PaginationFooter
          totalItems={totalItems}
          currentPage={currentPage}
          totalPages={totalPages}
          pageSize={pageSize}
          onPageChange={onPageChange}
        />
      </Card>

      <div className="flex flex-col gap-4 xl:hidden">
        <ul className="flex flex-col gap-4">
          {analyses.map((analysis) => (
            <li key={analysis.id}>
              <AnalysisMobileCard
                analysis={analysis}
                onPrimaryAction={onPrimaryAction}
                onMenuAction={onMenuAction}
              />
            </li>
          ))}
        </ul>

        <Card className="!rounded-lg !px-4 !py-3">
          <PaginationFooter
            totalItems={totalItems}
            currentPage={currentPage}
            totalPages={totalPages}
            pageSize={pageSize}
            onPageChange={onPageChange}
            compact
          />
        </Card>
      </div>
    </section>
  );
}

function AnalysisTableRow({
  analysis,
  onPrimaryAction,
  onMenuAction,
}: {
  analysis: UserAnalysis;
  onPrimaryAction: (analysis: UserAnalysis) => void;
  onMenuAction: (action: AnalysisMenuAction, analysis: UserAnalysis) => void;
}) {
  const formattedDate = formatAnalysisDate(analysis.analyzedAt);
  const primaryAction = getPrimaryAction(analysis);
  const ActionIcon = primaryAction.icon;

  return (
    <tr className="border-b border-placeholder last:border-b-0">
      <td className="px-4 py-4 pl-6 xl:px-5 xl:pl-6">
        <AnalysisIdentity analysis={analysis} />
      </td>
      <td className="px-4 py-4 xl:px-5">
        <span className="flex min-w-0 items-start gap-2 font-heading text-[11px] font-medium leading-relaxed text-text-secondary xl:text-[12px]">
          <LuMapPin className="mt-0.5 h-4 w-4 shrink-0" />
          <span className="line-clamp-2">{analysis.collectionPoint}</span>
        </span>
      </td>
      <td className="px-4 py-4 xl:px-5">
        <span className="block font-heading text-[12px] font-semibold text-text-primary">
          {formattedDate.date}
        </span>
        <span className="mt-1 block font-heading text-[11px] font-medium text-text-secondary">
          {formattedDate.time}
        </span>
      </td>
      <td className="px-4 py-4 xl:px-5">
        <AnalysisStatusBadge status={analysis.status} />
      </td>
      <td className="px-4 py-4 pr-6 xl:px-5 xl:pr-6">
        <span className="flex items-center justify-between gap-1">
          <button
            type="button"
            onClick={() => onPrimaryAction(analysis)}
            className="inline-flex min-h-9 min-w-0 items-center gap-2 rounded-md px-1 font-heading text-[11px] font-bold text-primary transition-colors hover:text-secondary focus:outline-none focus:ring-2 focus:ring-primary/30 xl:text-[12px]"
          >
            <ActionIcon className="h-4 w-4 shrink-0" />
            <span className="truncate">{primaryAction.label}</span>
          </button>
          <AnalysisActionsMenu analysis={analysis} onAction={onMenuAction} />
        </span>
      </td>
    </tr>
  );
}

function AnalysisMobileCard({
  analysis,
  onPrimaryAction,
  onMenuAction,
}: {
  analysis: UserAnalysis;
  onPrimaryAction: (analysis: UserAnalysis) => void;
  onMenuAction: (action: AnalysisMenuAction, analysis: UserAnalysis) => void;
}) {
  const formattedDate = formatAnalysisDate(analysis.analyzedAt);
  const primaryAction = getPrimaryAction(analysis);
  const ActionIcon = primaryAction.icon;

  return (
    <Card className="!rounded-lg !px-4 !py-4 sm:!px-5">
      <div className="flex items-start justify-between gap-3">
        <AnalysisIdentity analysis={analysis} />
        <AnalysisActionsMenu analysis={analysis} onAction={onMenuAction} />
      </div>

      <dl className="mt-4 grid gap-4 border-y border-placeholder py-4 sm:grid-cols-2">
        <div>
          <dt className="font-heading text-[10px] font-bold uppercase tracking-wide text-text-secondary">
            Ponto de coleta
          </dt>
          <dd className="mt-1.5 flex items-start gap-2 font-heading text-[12px] font-medium leading-relaxed text-text-primary">
            <LuMapPin className="mt-0.5 h-4 w-4 shrink-0 text-text-secondary" />
            {analysis.collectionPoint}
          </dd>
        </div>
        <div>
          <dt className="font-heading text-[10px] font-bold uppercase tracking-wide text-text-secondary">
            Data da análise
          </dt>
          <dd className="mt-1.5 font-heading text-[12px] font-semibold text-text-primary">
            {formattedDate.date} às {formattedDate.time}
          </dd>
        </div>
      </dl>

      <div className="mt-4 flex flex-col gap-3 min-[420px]:flex-row min-[420px]:items-center min-[420px]:justify-between">
        <AnalysisStatusBadge status={analysis.status} />
        <Button
          variant="outline"
          onClick={() => onPrimaryAction(analysis)}
          className="h-10 border-primary px-4 text-primary min-[420px]:w-auto"
        >
          <ActionIcon className="h-4 w-4" />
          {primaryAction.label}
        </Button>
      </div>
    </Card>
  );
}

function AnalysisIdentity({ analysis }: { analysis: UserAnalysis }) {
  return (
    <span className="flex min-w-0 items-center gap-3">
      <span className="inline-flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-primary/10 text-primary">
        <LuWaves className="h-5 w-5" />
      </span>
      <span className="min-w-0">
        <strong className="block truncate font-heading text-[12px] font-bold text-text-primary xl:text-[13px]">
          {analysis.waterBodyName}
        </strong>
        <span className="mt-1 block truncate font-heading text-[10px] font-medium text-text-secondary xl:text-[11px]">
          {analysis.city}, {analysis.state}
        </span>
      </span>
    </span>
  );
}

function PaginationFooter({
  totalItems,
  currentPage,
  totalPages,
  pageSize,
  onPageChange,
  compact = false,
}: {
  totalItems: number;
  currentPage: number;
  totalPages: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  compact?: boolean;
}) {
  const firstItem = (currentPage - 1) * pageSize + 1;
  const lastItem = Math.min(currentPage * pageSize, totalItems);
  const pages = Array.from({ length: totalPages }, (_, index) => index + 1);
  const paginationButtonClassName =
    "inline-flex h-9 min-w-9 items-center justify-center rounded-md border border-placeholder bg-white px-2 font-heading text-[11px] font-bold text-text-primary transition-colors hover:border-primary hover:text-primary focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:cursor-not-allowed disabled:opacity-40";

  return (
    <footer
      className={`flex items-center gap-3 ${
        compact
          ? "flex-col sm:flex-row sm:justify-between"
          : "justify-between border-t border-placeholder px-6 py-4"
      }`}
    >
      <p className="font-heading text-[10px] font-medium text-text-secondary sm:text-[11px]">
        Mostrando {firstItem} a {lastItem} de {totalItems} análises
      </p>

      <nav aria-label="Paginação das análises" className="flex items-center gap-2">
        <button
          type="button"
          aria-label="Página anterior"
          disabled={currentPage === 1}
          onClick={() => onPageChange(currentPage - 1)}
          className={paginationButtonClassName}
        >
          <LuChevronLeft className="h-4 w-4" />
        </button>

        {pages.map((page) => (
          <button
            key={page}
            type="button"
            aria-label={`Ir para a página ${page}`}
            aria-current={page === currentPage ? "page" : undefined}
            onClick={() => onPageChange(page)}
            className={`${paginationButtonClassName} ${
              page === currentPage
                ? "border-primary bg-primary text-white hover:text-white"
                : ""
            }`}
          >
            {page}
          </button>
        ))}

        <button
          type="button"
          aria-label="Próxima página"
          disabled={currentPage === totalPages}
          onClick={() => onPageChange(currentPage + 1)}
          className={paginationButtonClassName}
        >
          <LuChevronRight className="h-4 w-4" />
        </button>
      </nav>
    </footer>
  );
}

function getPrimaryAction(analysis: UserAnalysis): {
  label: string;
  icon: IconType;
} {
  switch (analysis.status) {
    case "Processando":
      return { label: "Acompanhar", icon: LuEye };
    case "Rascunho":
      return { label: "Continuar edição", icon: LuPencil };
    default:
      return { label: "Ver resultados", icon: LuChartNoAxesColumnIncreasing };
  }
}

function formatAnalysisDate(analyzedAt: string) {
  const [dateValue, timeValue = ""] = analyzedAt.split("T");
  const [year, month, day] = dateValue.split("-");

  return {
    date: `${day}/${month}/${year}`,
    time: timeValue.slice(0, 5),
  };
}
