"use client";

import { useEffect, useRef, useState } from "react";
import { LuChevronLeft, LuChevronRight, LuLoaderCircle } from "react-icons/lu";
import type { PDFDocumentProxy } from "pdfjs-dist";
import { Button } from "@/components/ui/Button";

export function PdfPreviewPages({ url }: { url: string }) {
  const [document, setDocument] = useState<PDFDocumentProxy | null>(null);
  const [page, setPage] = useState(1);
  const [width, setWidth] = useState(0);
  const [rendering, setRendering] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const container = useRef<HTMLDivElement>(null);
  const canvas = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    const element = container.current;
    if (!element) return;
    const observer = new ResizeObserver(entries => setWidth(Math.max(100, Math.floor(entries[0].contentRect.width))));
    observer.observe(element);
    return () => observer.disconnect();
  }, []);

  useEffect(() => {
    let cancelled = false;
    let destroy: (() => Promise<void>) | undefined;
    void (async () => {
      try {
        const pdfjs = await import("pdfjs-dist");
        pdfjs.GlobalWorkerOptions.workerSrc = new URL("pdfjs-dist/build/pdf.worker.min.mjs", import.meta.url).toString();
        const task = pdfjs.getDocument({ url });
        destroy = () => task.destroy();
        const loaded = await task.promise;
        if (!cancelled) setDocument(loaded);
      } catch {
        if (!cancelled) setError("Não foi possível exibir a prévia. O PDF pode ser aberto ou baixado pelos botões acima.");
      }
    })();
    return () => { cancelled = true; void destroy?.(); };
  }, [url]);

  useEffect(() => {
    if (!document || !width) return;
    let cancelled = false;
    let cancelRender: (() => void) | undefined;
    void (async () => {
      try {
        setRendering(true);
        const pdfPage = await document.getPage(page);
        if (cancelled || !canvas.current) return;
        const base = pdfPage.getViewport({ scale: 1 });
        const ratio = Math.min(window.devicePixelRatio || 1, 2);
        const viewport = pdfPage.getViewport({ scale: width / base.width * ratio });
        const element = canvas.current;
        element.width = Math.ceil(viewport.width); element.height = Math.ceil(viewport.height);
        const task = pdfPage.render({ canvas: element, viewport });
        cancelRender = () => task.cancel();
        await task.promise;
      } catch {
        if (!cancelled) setError("Não foi possível renderizar esta página. Use Abrir PDF para visualizar o arquivo.");
      } finally {
        if (!cancelled) setRendering(false);
      }
    })();
    return () => { cancelled = true; cancelRender?.(); };
  }, [document, page, width]);

  return <div className="space-y-3">
    <nav aria-label="Páginas da prévia PDF" className="flex flex-wrap items-center justify-between gap-2">
      <Button variant="outline" className="!h-9 !px-3" disabled={!document || page === 1} onClick={() => setPage(value => value - 1)} aria-label="Página anterior do PDF"><LuChevronLeft aria-hidden="true" /></Button>
      <span aria-live="polite" className="text-xs text-text-secondary">{document ? `Página ${page} de ${document.numPages}` : "Preparando páginas..."}</span>
      <Button variant="outline" className="!h-9 !px-3" disabled={!document || page === document.numPages} onClick={() => setPage(value => value + 1)} aria-label="Próxima página do PDF"><LuChevronRight aria-hidden="true" /></Button>
    </nav>
    {error && <p role="alert" className="rounded-md bg-warning/10 p-3 text-xs">{error}</p>}
    <div ref={container} className="relative min-h-48 overflow-hidden rounded-lg border border-placeholder bg-background/50">
      {(rendering || !document) && !error && <p role="status" className="absolute inset-x-0 top-8 flex items-center justify-center gap-2 bg-white/90 p-3 text-xs text-text-secondary"><LuLoaderCircle aria-hidden="true" className="h-4 w-4 animate-spin" />Renderizando página...</p>}
      <canvas ref={canvas} aria-label={`Página ${page} do relatório. O texto completo está disponível no PDF para abrir ou baixar.`} role="img" className="block h-auto w-full bg-white" />
    </div>
  </div>;
}
