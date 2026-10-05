export default function MonitoringSkeleton() {
  return (
    <section aria-label="Carregando monitoramento" className="space-y-6">
      <p role="status" className="text-sm text-text-secondary">
        Carregando dados do corpo hídrico...
      </p>

      <div aria-hidden="true" className="space-y-6 motion-safe:animate-pulse">
        <div className="h-8 w-52 rounded bg-primary/10" />
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          {Array.from({ length: 4 }, (_, index) => (
            <div key={index} className="h-28 rounded-xl bg-primary/10" />
          ))}
        </div>
        <div className="h-72 rounded-xl bg-primary/10" />
      </div>
    </section>
  );
}