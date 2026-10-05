"use client";

import Link from "next/link";
import { Button } from "@/components/ui/Button";

type WaterBodyErrorProps = {
  error: Error & { digest?: string };
  reset: () => void;
};

export default function WaterBodyError({ error, reset }: WaterBodyErrorProps) {
  return (
    <section aria-labelledby="monitoring-error-title" className="space-y-4 rounded-xl border border-placeholder p-6">
      <h1 id="monitoring-error-title" className="font-heading text-xl font-bold">
        Não foi possível exibir o monitoramento
      </h1>
      <p role="alert" className="text-sm text-text-secondary">
        Ocorreu uma falha ao carregar esta tela. Tente novamente ou recarregue a página.
      </p>
      {error.digest && (
        <p className="text-xs text-text-secondary">Referência: {error.digest}</p>
      )}
      <div className="flex flex-wrap gap-3">
        <Button onClick={reset}>Tentar novamente</Button>
        <Button variant="outline" onClick={() => window.location.reload()}>
          Recarregar página
        </Button>
      </div>
      <Link href="/water-bodies" className="inline-block text-primary underline">
        Voltar aos corpos hídricos
      </Link>
    </section>
  );
}