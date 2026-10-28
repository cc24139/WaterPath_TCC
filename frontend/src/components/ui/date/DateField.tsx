"use client";

import { useCallback, useRef, useState } from "react";
import { LuCalendarDays } from "react-icons/lu";
import { DatePickerDialog } from "./DatePickerDialog";
import { WaterCalendar } from "./WaterCalendar";
import { calendarToday, dateError, dateInputText, displayDate, parseDisplayDate, shiftDate } from "./dateUtils";

export interface DateFieldProps {
  id: string; label: string; value: string; onChange: (iso: string) => void;
  min?: string; max?: string; disabled?: boolean; required?: boolean; error?: string; hideLabel?: boolean;
}
export function DateField({ id, label, value, onChange, min = "1900-01-01", max, disabled, required, error, hideLabel }: DateFieldProps) {
  const [draft, setDraft] = useState({ source: value, text: displayDate(value) });
  const [touched, setTouched] = useState(false);
  const [open, setOpen] = useState(false);
  const trigger = useRef<HTMLButtonElement>(null);
  const close = useCallback(() => setOpen(false), []);
  const text = draft.source === value ? draft.text : displayDate(value);
  const parsed = parseDisplayDate(text);
  const invalid = error || (touched && text ? dateError(parsed ?? "", min, max) : null);
  const today = calendarToday();
  function choose(iso: string) {
    setDraft({ source: iso, text: displayDate(iso) }); setTouched(false); onChange(iso); close();
  }
  return <div className="min-w-0">
    <label htmlFor={id} className={hideLabel ? "sr-only" : "mb-2 block font-heading text-xs font-bold"}>{label}</label>
    <div className="relative">
      <input id={id} type="text" inputMode="numeric" autoComplete="off" placeholder="DD/MM/AAAA" maxLength={10} value={text}
        required={required} disabled={disabled} aria-invalid={!!invalid} aria-describedby={invalid ? `${id}-date-error` : undefined}
        className="wp-date-input pr-11" onBlur={() => setTouched(true)}
        onChange={event => {
          const next = dateInputText(event.target.value); const iso = parseDisplayDate(next);
          const committed = iso && !dateError(iso, min, max) ? iso : "";
          setDraft({ source: committed, text: next }); onChange(committed);
        }} />
      <button ref={trigger} type="button" disabled={disabled} onClick={() => setOpen(true)} aria-label={`Abrir calendário: ${label}`} aria-haspopup="dialog" aria-expanded={open}
        className="absolute right-1 top-1 flex h-9 w-9 items-center justify-center rounded-md text-primary hover:bg-primary/10 focus-visible:outline-primary disabled:cursor-not-allowed disabled:opacity-50"><LuCalendarDays aria-hidden="true" className="h-4 w-4" /></button>
    </div>
    {invalid && <p id={`${id}-date-error`} role="alert" className="mt-1.5 text-[11px] font-semibold text-contrast">{invalid}</p>}
    {open && <DatePickerDialog anchorRef={trigger} title="Escolher data" onClose={close}>
      <div className="mb-3 flex gap-2">
        {[{ label: "Hoje", date: today }, { label: "Ontem", date: shiftDate(today, -1) }].map(shortcut => <button key={shortcut.label} type="button" disabled={!!dateError(shortcut.date, min, max)} onClick={() => choose(shortcut.date)} className="rounded-full border border-primary/20 px-4 py-2 text-xs font-semibold text-primary hover:bg-primary/5 focus-visible:outline-primary disabled:opacity-40">{shortcut.label}</button>)}
      </div>
      <WaterCalendar mode="single" value={value} min={min} max={max ?? `${new Date().getFullYear() + 10}-12-31`} today={today} onSelect={choose} />
      <p className="mt-3 border-t border-placeholder pt-3 text-center text-[11px] text-text-secondary">Você também pode digitar a data no campo.</p>
    </DatePickerDialog>}
  </div>;
}
