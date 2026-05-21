import { LuFileText, LuInfo, LuRefreshCw } from "react-icons/lu";

interface ReportActionsProps {
  onReset: () => void;
  onGenerate: () => void;
}

export function ReportActions({ onReset, onGenerate }: ReportActionsProps) {
  return (
    <footer className="grid grid-cols-1 gap-3 lg:grid-cols-[auto_minmax(0,1fr)_auto] lg:items-center">
      <button
        type="button"
        onClick={onReset}
        className="inline-flex h-12 items-center justify-center gap-2 rounded-md border border-primary bg-white px-5 font-heading text-[12px] font-semibold text-primary transition-colors hover:bg-[#E9FBFE]"
      >
        <LuRefreshCw className="h-4 w-4" />
        Limpar configurações
      </button>

      <div className="flex min-h-12 min-w-0 items-center gap-3 rounded-md bg-[#E4E4E4] px-4 py-3 font-heading text-[10px] font-medium leading-relaxed text-[#767676] sm:text-[11px]">
        <LuInfo className="h-4 w-4 shrink-0 text-primary" />
        <span>
          As configurações escolhidas determinam o conteúdo, o nível de detalhe
          e o tamanho do relatório. O preview será atualizado após gerar o
          relatório.
        </span>
      </div>

      <button
        type="button"
        onClick={onGenerate}
        className="inline-flex h-12 items-center justify-center gap-2 rounded-md bg-primary px-6 font-heading text-[12px] font-bold text-white shadow-[0_4px_14px_rgba(23,166,191,0.3)] transition-colors hover:bg-secondary"
      >
        <LuFileText className="h-4 w-4" />
        Gerar Relatório e atualizar preview
      </button>
    </footer>
  );
}
