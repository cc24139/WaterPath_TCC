"use client";

import { useRouter } from "next/navigation";
import { LuSquarePlus } from "react-icons/lu";

import { SideBar } from "@/components/layout/SideBar";
import { Button } from "@/components/ui/Button";
import { AnalysisFilters } from "@/features/my-analyses/components/AnalysisFilters";
import { AnalysisStatsCards } from "@/features/my-analyses/components/AnalysisStatsCards";
import { UserAnalysesList } from "@/features/my-analyses/components/UserAnalysesList";
import { useMyAnalyses } from "@/features/my-analyses/hooks/useMyAnalyses";
import type {
  AnalysisMenuAction,
  UserAnalysis,
} from "@/features/my-analyses/types/myAnalyses";

export function MyAnalysesPage() {
  const router = useRouter();
  const {
    analyses,
    visibleAnalyses,
    filteredCount,
    stats,
    search,
    statusFilter,
    sortBy,
    activePage,
    totalPages,
    pageSize,
    setSearch,
    setStatusFilter,
    setSortBy,
    setCurrentPage,
    clearFilters,
    removeAnalysis,
  } = useMyAnalyses();

  function goToNewAnalysis() {
    router.push("/add-analysis");
  }

  function handlePrimaryAction(analysis: UserAnalysis) {
    switch (analysis.status) {
      case "Processando":
        window.alert(
          `A análise de “${analysis.waterBodyName}” ainda está sendo processada.`
        );
        break;
      case "Rascunho":
        router.push(`/add-analysis?draft=${analysis.id}`);
        break;
      default:
        router.push(`/water-bodies/${analysis.waterBodyId}`);
    }
  }

  function handleMenuAction(
    action: AnalysisMenuAction,
    analysis: UserAnalysis
  ) {
    switch (action) {
      case "details":
        window.alert(
          `Os detalhes de “${analysis.waterBodyName}” serão abertos aqui quando a rota de detalhes estiver disponível.`
        );
        break;
      case "edit":
        router.push(`/add-analysis?edit=${analysis.id}`);
        break;
      case "duplicate":
        window.alert(
          `A duplicação de “${analysis.waterBodyName}” está preparada para a futura integração com a API.`
        );
        break;
      case "delete":
        if (
          window.confirm(
            `Deseja excluir a análise de “${analysis.waterBodyName}”? Esta ação afeta apenas os dados mockados desta sessão.`
          )
        ) {
          removeAnalysis(analysis.id);
        }
        break;
    }
  }

  return (
    <div className="min-h-screen bg-background lg:flex">
      <SideBar
        variant="analysis-registration"
        subtitle="Área de cadastro"
        activeHref="/my-analyses"
      />

      <main className="min-w-0 flex-1 px-4 py-5 sm:px-6 lg:px-8 xl:px-10">
        <div className="mx-auto flex w-full max-w-[1500px] flex-col gap-6 lg:gap-7">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div className="min-w-0">
              <h1 className="font-heading text-[28px] font-bold leading-tight text-text-primary sm:text-[32px]">
                Minhas análises
              </h1>
              <p className="mt-2 max-w-3xl font-heading text-[12px] font-medium leading-relaxed text-text-secondary sm:text-[13px]">
                Acompanhe, revise e acesse as análises cadastradas por você.
              </p>
            </div>

            <Button
              onClick={goToNewAnalysis}
              className="w-full px-6 sm:w-auto sm:min-w-40"
            >
              <LuSquarePlus className="h-4 w-4" />
              Nova análise
            </Button>
          </div>

          <AnalysisFilters
            search={search}
            statusFilter={statusFilter}
            sortBy={sortBy}
            onSearchChange={setSearch}
            onStatusChange={setStatusFilter}
            onSortChange={setSortBy}
          />

          <AnalysisStatsCards stats={stats} />

          <UserAnalysesList
            analyses={visibleAnalyses}
            totalItems={filteredCount}
            totalAnalyses={analyses.length}
            currentPage={activePage}
            totalPages={totalPages}
            pageSize={pageSize}
            onPageChange={setCurrentPage}
            onPrimaryAction={handlePrimaryAction}
            onMenuAction={handleMenuAction}
            onCreateAnalysis={goToNewAnalysis}
            onClearFilters={clearFilters}
          />
        </div>
      </main>
    </div>
  );
}
