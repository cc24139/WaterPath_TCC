"use client";

import { useState } from "react";
import { DateField } from "./DateField";
import { calendarToday, normalizeTime, timeInputText } from "./dateUtils";

export function DateTimeField({ id, label, value, onChange, required, disabled, error, helper, className = "" }: {
  id: string; label: string; value: string; onChange: (value: string) => void; required?: boolean; disabled?: boolean; error?: string; helper?: string; className?: string;
}) {
  const [date = "", time = ""] = value.split("T");
  const [timeTouched, setTimeTouched] = useState(false);
  const [now, setNow] = useState(() => Date.now());
  const invalidTime = timeTouched && time && !normalizeTime(time) ? "Use um horário entre 00:00 e 23:59." : null;
  const future = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/.test(value) && new Date(value).getTime() > now;
  return <div className={`min-w-0 ${className}`}>
    <label htmlFor={id} className="mb-2 block font-heading text-xs font-bold">{label}</label>
    <div className="grid grid-cols-[minmax(0,1fr)_88px] items-start gap-2">
      <DateField id={id} label="Data da coleta" value={date} onChange={next => { setNow(Date.now()); onChange(`${next}${time ? `T${time}` : ""}`); }} min="1900-01-01" max={calendarToday()} required={required} disabled={disabled} hideLabel />
      <div><label htmlFor={`${id}-time`} className="sr-only">Horário da coleta</label><input id={`${id}-time`} type="text" inputMode="numeric" autoComplete="off" maxLength={5} placeholder="HH:mm" value={time} disabled={disabled} required={required} aria-invalid={!!(invalidTime || error || future)} aria-describedby={error || invalidTime || future ? `${id}-error` : `${id}-helper`}
        onChange={event => { setNow(Date.now()); const text = timeInputText(event.target.value); onChange(`${date}${text ? `T${text}` : ""}`); }}
        onBlur={() => { setTimeTouched(true); setNow(Date.now()); }} className="wp-date-input !px-2 text-center" /></div>
    </div>
    {(error || invalidTime || future) && <p id={`${id}-error`} role="alert" className="mt-1.5 text-[11px] font-semibold text-contrast">{error || invalidTime || "A coleta não pode ter data ou horário futuro."}</p>}
    {helper && <p id={`${id}-helper`} className="mt-1.5 text-[10px] text-text-secondary">{helper}</p>}
  </div>;
}
