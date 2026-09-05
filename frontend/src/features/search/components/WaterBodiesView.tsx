"use client";

import { useMemo, useState } from "react";
import Link from "next/link";

import { Header } from "@/components/layout/Header";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import type { River } from "@/features/search/types/river";
import { useAuthSession } from "@/features/auth/hooks/useAuthSession";
import { RiverList } from "@/features/search/components/RiverList";
import { RiverListSelect } from "@/features/search/components/RiverListSelect";
import { RiverListToolBar, type RiverFilters } from "@/features/search/components/RiverListToolBar";
import { Footer } from "@/components/layout/Footer";

interface WaterBodiesViewProps {
  rivers: River[];
  loading?: boolean;
  error?: string | null;
  warnings?: string[];
  retry?: () => void;
  demo?: boolean;
}

export function WaterBodiesView({
  rivers,
  loading = false,
  error = null,
  warnings = [],
  retry,
  demo = false,
}: WaterBodiesViewProps) {
  const [selectedRiverId, setSelectedRiverId] = useState<string | undefined>();
  const [filters, setFilters] = useState<RiverFilters>({
    query: "", status: "", location: "", onlyMine: false,
  });
  const { user, isAuthenticated } = useAuthSession();
  const canFilterMine = isAuthenticated && !demo;
  const locations = useMemo(
    () => [...new Set(rivers.map((river) => river.location))]
      .sort((a, b) => a.localeCompare(b, "pt-BR")),
    [rivers]
  );

  const filteredRivers = useMemo(() => {
    const normalize = (value: string) => value.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLocaleLowerCase("pt-BR");
    const query = normalize(filters.query.trim());
    return rivers.filter((river) =>
      normalize(`${river.name} ${river.location}`).includes(query)
      && (!filters.status || river.status === filters.status)
      && (!filters.location || river.location === filters.location)
      && (!filters.onlyMine || !canFilterMine || (user && river.userIds.includes(user.id)))
    );
  }, [rivers, filters, user, canFilterMine]);
  const activeRiverId = filteredRivers.some((river) => river.id === selectedRiverId) ? selectedRiverId : undefined;
  const visibleRivers = activeRiverId ? filteredRivers.filter((river) => river.id === activeRiverId) : filteredRivers;

  return (
    <div className="flex min-h-dvh flex-col bg-background text-text-primary">
      <Header />

      <main className="mx-auto flex w-full max-w-7xl flex-1 flex-col px-3 py-5 sm:px-6 sm:py-6 lg:px-8 pb-4">
        {demo && (
          <Card className="mb-5 border border-primary/30">
            <p className="font-heading text-base font-bold text-primary">Prévia com dados fictícios</p>
            <p className="mt-2 text-sm text-text-secondary">
              Explore seis exemplos: histórico completo, valores ausentes, uma única coleta,
              IQA baixo, múltiplos IQAs sem data e um corpo hídrico sem medições.
              Use os filtros e troque o parâmetro de cada gráfico para comparar.
            </p>
            <Link href="/water-bodies" className="mt-3 inline-flex rounded text-sm font-semibold text-primary underline underline-offset-4 focus-visible:outline-primary">
              Voltar aos dados reais
            </Link>
          </Card>
        )}
        <RiverListToolBar
          filters={filters}
          onChange={(nextFilters) => {
            setFilters(nextFilters);
            setSelectedRiverId(undefined);
          }}
          locations={locations}
          isAuthenticated={canFilterMine}
          disabled={loading}
        />

        <details className="mt-4 text-xs text-text-secondary">
          <summary className="cursor-pointer rounded font-semibold text-primary focus-visible:outline-primary">Como interpretar os dados</summary>
          <div className="mt-2 space-y-2">
            <p>Os gráficos mostram apenas medições registradas, nas datas disponíveis. Intervalos sem coleta não representam medições nem previsões. As escalas de turbidez e oxigênio se ajustam aos valores de cada corpo hídrico.</p>
            <p>O IQA é apresentado na escala de 0 a 100, sem data informada. A classificação visual segue as faixas usadas no projeto: Crítica abaixo de 50; Atenção de 50 a menos de 75; Boa de 75 a menos de 90; Ótima a partir de 90. Ela não indica, por si só, potabilidade.</p>
            <p>As unidades de turbidez e oxigênio não estão informadas nos registros. Confirme a unidade das medições antes de comparar fontes diferentes.</p>
          </div>
        </details>

        {loading && (
          <Card className="mt-6">
            <p role="status" className="text-sm text-text-secondary">Carregando corpos hídricos e medições...</p>
          </Card>
        )}
        {error && (
          <Card className="mt-6">
            <p role="alert" className="mb-3 text-sm text-text-primary">{error}</p>
            {retry && <Button variant="outline" onClick={retry}>Tentar novamente</Button>}
          </Card>
        )}
        {warnings.length > 0 && (
          <Card className="mt-6 border border-warning/40">
            <div role="status" className="space-y-2 text-sm text-text-secondary">
              {warnings.map((warning) => <p key={warning}>{warning}</p>)}
            </div>
            {retry && <Button className="mt-3" variant="outline" onClick={retry}>Recarregar dados</Button>}
          </Card>
        )}
        {!loading && !error && filteredRivers.length === 0 && (
          <Card className="mt-6">
            <p role="status" className="text-sm text-text-secondary">
              {rivers.length === 0 ? "Nenhum corpo hídrico cadastrado." : "Nenhum corpo hídrico corresponde aos filtros selecionados."}
            </p>
          </Card>
        )}

        {!loading && !error && filteredRivers.length > 0 && (
          <section className="mt-6 grid gap-5 sm:mt-8 sm:gap-7 lg:min-h-[520px] lg:flex-1 lg:grid-cols-[minmax(250px,300px)_minmax(0,1fr)] xl:gap-14">
            <aside className="flex justify-center lg:block">
              <RiverListSelect
                rivers={filteredRivers}
                selectedRiverId={activeRiverId}
                onSelectRiver={(river) =>
                  setSelectedRiverId((currentRiverId) =>
                    currentRiverId === river.id ? undefined : river.id
                  )
                }
                className="max-w-full sm:max-w-[360px] lg:h-full lg:max-w-none"
              />
            </aside>

            <div className="relative min-w-0 lg:min-h-0">
              <RiverList
                rivers={visibleRivers}
                selectedRiverId={activeRiverId}
                className="mx-auto max-w-[760px] lg:absolute lg:inset-0 lg:mx-0 lg:max-w-none lg:overflow-y-auto lg:overscroll-contain lg:px-1 lg:py-1 lg:[scrollbar-width:thin] lg:[scrollbar-color:var(--color-primary)_transparent]"
              />
            </div>
          </section>
        )}
      </main>
      <Footer />
    </div>
  );
}
