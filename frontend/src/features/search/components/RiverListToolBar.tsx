import { CiSearch } from "react-icons/ci";
import { Card } from "@/components/ui/Card";
import type { RiverStatus } from "../types/river";

export interface RiverFilters {
  query: string;
  status: string;
  location: string;
  onlyMine: boolean;
}

interface RiverListToolBarProps {
  filters: RiverFilters;
  onChange: (filters: RiverFilters) => void;
  locations: string[];
  isAuthenticated: boolean;
  disabled?: boolean;
}

const statuses: RiverStatus[] = ["Ótima", "Boa", "Atenção", "Crítica", "Sem classificação"];
const selectClassName = "min-w-0 rounded-md border border-placeholder bg-white px-2 py-2 text-xs text-text-primary focus-visible:outline-primary disabled:opacity-50";

export function RiverListToolBar({ filters, onChange, locations, isAuthenticated, disabled }: RiverListToolBarProps) {
  return (
    <Card className="mx-auto flex w-full flex-col gap-4 !px-4 !py-4 sm:!px-5 lg:flex-row lg:items-center">
      <div className="lg:max-w-56">
        <h1 className="font-heading text-xl leading-tight text-text-primary">Corpos hídricos</h1>
        <p className="mt-1 text-xs text-text-secondary">Consulte os registros e medições disponíveis.</p>
      </div>
      <div className="relative min-w-0 flex-1">
        <input type="search" aria-label="Buscar por nome ou localização" placeholder="Pesquise por nome ou localização" value={filters.query} disabled={disabled} onChange={(event) => onChange({ ...filters, query: event.target.value })} className="h-10 w-full rounded-full border border-placeholder bg-white pl-4 pr-10 text-xs text-text-primary placeholder:text-text-secondary focus-visible:outline-primary disabled:opacity-50" />
        <CiSearch aria-hidden="true" size={18} className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-text-primary" />
      </div>
      <div className="flex flex-wrap items-center gap-3">
        <select aria-label="Filtrar por classificação do IQA" value={filters.status} disabled={disabled} onChange={(event) => onChange({ ...filters, status: event.target.value })} className={selectClassName}>
          <option value="">Todos os status</option>
          {statuses.map((status) => <option key={status}>{status}</option>)}
        </select>
        <select aria-label="Filtrar por localização" value={filters.location} disabled={disabled} onChange={(event) => onChange({ ...filters, location: event.target.value })} className={`${selectClassName} max-w-48`}>
          <option value="">Todas as localizações</option>
          {locations.map((location) => <option key={location}>{location}</option>)}
        </select>
        <label className={`flex items-center gap-2 text-xs text-text-secondary ${!isAuthenticated ? "opacity-60" : "cursor-pointer"}`} title={!isAuthenticated ? "Entre para filtrar seus corpos hídricos" : undefined}>
          <input type="checkbox" checked={isAuthenticated && filters.onlyMine} disabled={disabled || !isAuthenticated} onChange={(event) => onChange({ ...filters, onlyMine: event.target.checked })} className="h-4 w-4 accent-primary focus-visible:outline-primary" />
          Meus corpos hídricos
        </label>
      </div>
    </Card>
  );
}
