import { LuDownload, LuExternalLink, LuFileText, LuLoaderCircle } from "react-icons/lu";
import { Card } from "@/components/ui/Card";
import { PdfPreviewPages } from "./PdfPreviewPages";

export function ReportPreview({ url, filename, loading, stale }: { url: string | null; filename: string; loading: boolean; stale: boolean }) {
  const linkClass = "inline-flex min-h-10 items-center justify-center gap-2 rounded-md border border-primary px-3 py-2 text-xs font-bold text-primary hover:bg-primary/5 focus:outline-primary";
  return <Card className="min-w-0 !p-4 sm:!p-5">
    <div className="mb-4 flex flex-wrap items-start justify-between gap-3">
      <div><h2 className="font-heading text-base font-bold">Prévia do relatório</h2><p className="mt-1 text-xs text-text-secondary">Visualização e download do mesmo PDF.</p></div>
      {url && !loading && !stale && <div className="flex flex-wrap gap-2"><a href={url} target="_blank" rel="noopener noreferrer" className={linkClass}><LuExternalLink aria-hidden="true" />Abrir PDF</a><a href={url} download={filename} className={linkClass}><LuDownload aria-hidden="true" />Baixar PDF</a></div>}
    </div>
    {stale && <p role="status" className="mb-3 rounded-md bg-warning/10 p-3 text-xs leading-relaxed">As configurações mudaram. Gere novamente para atualizar a prévia e liberar o download.</p>}
    {loading ? <div role="status" className="flex min-h-[440px] flex-col items-center justify-center gap-4 rounded-lg bg-background/60 px-5 text-center"><LuLoaderCircle aria-hidden="true" className="h-9 w-9 animate-spin text-primary" /><p className="text-sm text-text-secondary">Organizando dados e preparando seu PDF...</p></div>
      : url ? <div className={`space-y-3 ${stale ? "opacity-65" : ""}`}><PdfPreviewPages key={url} url={url} /><p className="text-[11px] leading-relaxed text-text-secondary">Navegue pelas páginas para conferir o documento antes de compartilhar.</p></div>
      : <div className="flex min-h-[440px] flex-col items-center justify-center gap-4 rounded-xl border border-dashed border-placeholder bg-background/40 p-6 text-center"><span className="rounded-full bg-primary/10 p-5"><LuFileText aria-hidden="true" className="h-10 w-10 text-primary" /></span><h3 className="font-heading text-lg font-bold">Do monitoramento ao documento</h3><p className="max-w-sm text-sm leading-relaxed text-text-secondary">Escolha o período e as seções. Seu relatório reunirá medições, riscos, metais e evidências disponíveis para este corpo hídrico.</p><span className="rounded-full bg-white px-4 py-2 text-xs text-text-secondary">PDF A4 · identidade WaterPath · dados reais</span></div>}
  </Card>;
}
