"use client";

import { useMemo, useState } from "react";

import {
  analysesPerPage,
  mockUserAnalyses,
} from "@/features/my-analyses/constants/mockUserAnalyses";
import type {
  AnalysisSortOption,
  AnalysisStats,
  AnalysisStatusFilter,
} from "@/features/my-analyses/types/myAnalyses";

export function useMyAnalyses() {
  const [analyses, setAnalyses] = useState(mockUserAnalyses);
  const [search, setSearchState] = useState("");
  const [statusFilter, setStatusFilterState] =
    useState<AnalysisStatusFilter>("Todos os status");
  const [sortBy, setSortByState] =
    useState<AnalysisSortOption>("newest");
  const [currentPage, setCurrentPage] = useState(1);

  const stats = useMemo<AnalysisStats>(() => {
    return {
      total: analyses.length,
      processing: analyses.filter((item) => item.status === "Processando")
        .length,
      completed: analyses.filter(
        (item) => item.status === "Concluída" || item.status === "Atenção"
      ).length,
      drafts: analyses.filter((item) => item.status === "Rascunho").length,
    };
  }, [analyses]);

  const filteredAnalyses = useMemo(() => {
    const normalizedSearch = normalizeSearchTerm(search.trim());
    const matchingAnalyses = analyses.filter((analysis) => {
      const matchesStatus =
        statusFilter === "Todos os status" ||
        analysis.status === statusFilter;
      const searchableText = normalizeSearchTerm(
        [
          analysis.waterBodyName,
          analysis.city,
          analysis.state,
          analysis.collectionPoint,
        ].join(" ")
      );

      return matchesStatus && searchableText.includes(normalizedSearch);
    });

    return matchingAnalyses.toSorted((first, second) => {
      switch (sortBy) {
        case "oldest":
          return first.analyzedAt.localeCompare(second.analyzedAt);
        case "water-body":
          return first.waterBodyName.localeCompare(second.waterBodyName, "pt-BR");
        case "status":
          return first.status.localeCompare(second.status, "pt-BR");
        default:
          return second.analyzedAt.localeCompare(first.analyzedAt);
      }
    });
  }, [analyses, search, sortBy, statusFilter]);

  const totalPages = Math.max(
    1,
    Math.ceil(filteredAnalyses.length / analysesPerPage)
  );
  const activePage = Math.min(currentPage, totalPages);
  const pageStartIndex = (activePage - 1) * analysesPerPage;
  const visibleAnalyses = filteredAnalyses.slice(
    pageStartIndex,
    pageStartIndex + analysesPerPage
  );

  function setSearch(value: string) {
    setSearchState(value);
    setCurrentPage(1);
  }

  function setStatusFilter(value: AnalysisStatusFilter) {
    setStatusFilterState(value);
    setCurrentPage(1);
  }

  function setSortBy(value: AnalysisSortOption) {
    setSortByState(value);
    setCurrentPage(1);
  }

  function clearFilters() {
    setSearchState("");
    setStatusFilterState("Todos os status");
    setSortByState("newest");
    setCurrentPage(1);
  }

  function removeAnalysis(analysisId: string) {
    setAnalyses((currentAnalyses) =>
      currentAnalyses.filter((analysis) => analysis.id !== analysisId)
    );
  }

  return {
    analyses,
    visibleAnalyses,
    filteredCount: filteredAnalyses.length,
    stats,
    search,
    statusFilter,
    sortBy,
    activePage,
    totalPages,
    pageSize: analysesPerPage,
    setSearch,
    setStatusFilter,
    setSortBy,
    setCurrentPage,
    clearFilters,
    removeAnalysis,
  };
}

function normalizeSearchTerm(value: string) {
  return value
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLocaleLowerCase("pt-BR");
}
