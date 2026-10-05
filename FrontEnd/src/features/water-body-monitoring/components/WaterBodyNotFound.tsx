import Link from "next/link";

export default function WaterBodyNotFound() {
  return (
    <section aria-labelledby="water-body-not-found-title" className="space-y-4 rounded-xl border border-placeholder p-6">
      <h1 id="water-body-not-found-title" className="font-heading text-xl font-bold">
        Corpo hídrico não encontrado
      </h1>
      <p className="text-sm text-text-secondary">
        Não encontramos um corpo hídrico para este endereço. Ele pode ter sido removido.
      </p>
      <Link href="/water-bodies" className="inline-block text-primary underline">
        Consultar corpos hídricos
      </Link>
    </section>
  );
}