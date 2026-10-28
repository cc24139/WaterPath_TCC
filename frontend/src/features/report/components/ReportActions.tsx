import { LuFileText, LuRefreshCw } from "react-icons/lu";
import { Button } from "@/components/ui/Button";

export function ReportActions({ onReset, onGenerate, loading, disabled }: { onReset: () => void; onGenerate: () => void; loading: boolean; disabled: boolean }) {
  return <footer className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
    <Button variant="outline" onClick={onReset} disabled={loading}><LuRefreshCw aria-hidden="true" />Restaurar configurações</Button>
    <Button onClick={onGenerate} isLoading={loading} disabled={disabled}><LuFileText aria-hidden="true" />{loading ? "Gerando documento..." : "Gerar relatório e atualizar prévia"}</Button>
  </footer>;
}
