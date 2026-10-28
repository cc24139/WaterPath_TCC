"use client";

import { useCallback, useId, useRef, useState } from "react";
import { LuArrowRight, LuCalendarDays, LuChevronDown } from "react-icons/lu";
import { Button } from "@/components/ui/Button";
import { DatePickerDialog } from "./DatePickerDialog";
import { WaterCalendar } from "./WaterCalendar";
import { calendarToday, dateInputText, displayDate, parseDisplayDate, periodPresets, rangeDays, rangeError } from "./dateUtils";

type Range = { from: string; to: string };
export function DateRangeField({ id, label, value, onChange, disabled, maxDays = 366, min = "1900-01-01", timeZone = "America/Sao_Paulo" }: {
  id: string; label: string; value: Range; onChange: (range: Range) => void; disabled?: boolean; maxDays?: number; min?: string; timeZone?: string;
}) {
  const [open, setOpen] = useState(false);
  const trigger = useRef<HTMLButtonElement>(null);
  const close = useCallback(() => setOpen(false), []);
  const today = calendarToday(timeZone);
  return <div className="min-w-0">
    <label htmlFor={id} className="sr-only">{label}</label>
    <button id={id} ref={trigger} type="button" disabled={disabled} onClick={() => setOpen(true)} aria-haspopup="dialog" aria-expanded={open} aria-describedby={`${id}-value`}
      className="flex min-h-14 w-full min-w-0 items-center gap-3 rounded-lg border border-placeholder bg-white px-3 py-3 text-left transition-colors hover:border-primary focus-visible:outline-primary disabled:cursor-not-allowed disabled:opacity-60">
      <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary"><LuCalendarDays aria-hidden="true" className="h-4 w-4" /></span>
      <span className="min-w-0 flex-1"><span className="mb-1 block text-[10px] font-semibold uppercase tracking-wide text-text-secondary">{label}</span><span id={`${id}-value`} className="flex flex-wrap items-center gap-x-1.5 gap-y-1 text-xs font-bold"><span>{displayDate(value.from) || "Data inicial"}</span><LuArrowRight aria-hidden="true" className="h-3 w-3 shrink-0 text-text-secondary" /><span className="sr-only">até</span><span>{displayDate(value.to) || "Data final"}</span></span></span>
      <LuChevronDown aria-hidden="true" className="h-4 w-4 shrink-0 text-text-secondary" />
    </button>
    {open && <DatePickerDialog anchorRef={trigger} title="Selecionar período" onClose={close}>
      <RangeEditor value={value} min={min} max={today} maxDays={maxDays} onCancel={close} onApply={range => { onChange(range); close(); }} />
    </DatePickerDialog>}
  </div>;
}

function RangeEditor({ value, min, max, maxDays, onApply, onCancel }: { value: Range; min: string; max: string; maxDays: number; onApply: (range: Range) => void; onCancel: () => void }) {
  const [start, setStart] = useState(displayDate(value.from));
  const [end, setEnd] = useState(displayDate(value.to));
  const [selectingEnd, setSelectingEnd] = useState(false);
  const inputId = useId();
  const from = parseDisplayDate(start) ?? "", to = parseDisplayDate(end) ?? "";
  const invalid = start.length === 10 && !from ? "Informe uma data inicial válida (DD/MM/AAAA)."
    : end.length === 10 && !to ? "Informe uma data final válida (DD/MM/AAAA)."
    : from && to ? rangeError(from, to, min, max, maxDays) : "Escolha as datas inicial e final.";
  const showError = !!invalid && !selectingEnd && (!!(from && to) || start.length === 10 || end.length === 10);
  function choose(date: string) {
    if (!selectingEnd || !from) { setStart(displayDate(date)); setEnd(""); setSelectingEnd(true); }
    else { setStart(displayDate(date < from ? date : from)); setEnd(displayDate(date < from ? from : date)); setSelectingEnd(false); }
  }
  return <div className="space-y-3">
    <div className="grid grid-cols-2 gap-2">
      {periodPresets(max).map(preset => <button key={preset.label} type="button" aria-pressed={from === preset.from && to === preset.to}
        onClick={() => { setStart(displayDate(preset.from)); setEnd(displayDate(preset.to)); setSelectingEnd(false); }}
        className={`rounded-md border px-2 py-2 text-[11px] font-semibold transition-colors hover:border-primary hover:bg-primary/5 hover:text-primary focus-visible:outline-primary ${from === preset.from && to === preset.to ? "border-primary bg-primary/5 text-primary" : "border-placeholder/80 text-text-secondary"}`}>{preset.label}</button>)}
    </div>
    <div className="grid grid-cols-2 gap-2">
      <label className="text-[11px] font-semibold" htmlFor={`${inputId}-from`}>De<input id={`${inputId}-from`} type="text" inputMode="numeric" autoComplete="off" maxLength={10} placeholder="DD/MM/AAAA" value={start} aria-invalid={showError} aria-describedby={`${inputId}-status`} onChange={e => { setStart(dateInputText(e.target.value)); setSelectingEnd(false); }} className="wp-date-input mt-1" /></label>
      <label className="text-[11px] font-semibold" htmlFor={`${inputId}-to`}>Até<input id={`${inputId}-to`} type="text" inputMode="numeric" autoComplete="off" maxLength={10} placeholder="DD/MM/AAAA" value={end} aria-invalid={showError} aria-describedby={`${inputId}-status`} onChange={e => { setEnd(dateInputText(e.target.value)); setSelectingEnd(false); }} className="wp-date-input mt-1" /></label>
    </div>
    <WaterCalendar mode="range" from={from} to={to} min={min} max={max} today={max} onSelect={choose} />
    <p id={`${inputId}-status`} aria-live="polite" className={`min-h-4 text-center text-[11px] ${showError ? "text-contrast" : "text-text-secondary"}`}>{selectingEnd ? "Agora escolha a data final." : invalid || `${rangeDays(from, to)} dias selecionados · horário de Brasília`}</p>
    <div className="flex justify-between gap-2 border-t border-placeholder pt-3"><Button variant="ghost" className="!h-10 !px-3" onClick={onCancel}>Cancelar</Button><Button className="!h-10 !px-4" disabled={!!invalid} onClick={() => onApply({ from, to })}>Aplicar período</Button></div>
  </div>;
}
