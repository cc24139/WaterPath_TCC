"use client";

import "@daypicker/react/style.css";
import "./date-picker.css";

import { useState } from "react";
import { DayPicker, type DateRange } from "@daypicker/react";
import { ptBR } from "@daypicker/react/locale";
import { calendarISO, toCalendarDate } from "./dateUtils";

type Props = { mode: "single"; value?: string; onSelect: (value: string) => void } | { mode: "range"; from?: string; to?: string; onSelect: (value: string) => void };
export function WaterCalendar(props: Props & { min: string; max: string; today: string }) {
  const selection = props.mode === "single" ? props.value : props.to || props.from;
  const initial = toCalendarDate(selection ?? "") ?? toCalendarDate(props.today)!;
  const selectionKey = selection ?? "";
  const [navigation, setNavigation] = useState({ key: selectionKey, month: initial });
  const common = {
    locale: ptBR, captionLayout: "dropdown" as const, navLayout: "after" as const,
    month: navigation.key === selectionKey ? navigation.month : initial,
    onMonthChange: (month: Date) => setNavigation({ key: selectionKey, month }),
    startMonth: toCalendarDate(props.min), endMonth: toCalendarDate(props.max),
    today: toCalendarDate(props.today), disabled: [{ before: toCalendarDate(props.min)! }, { after: toCalendarDate(props.max)! }],
    showOutsideDays: true, fixedWeeks: true, className: "wp-calendar", weekStartsOn: 0 as const,
    onDayClick: (date: Date, modifiers: { disabled?: boolean }) => { if (!modifiers.disabled) props.onSelect(calendarISO(date)); },
    labels: { labelNext: () => "Ir para o próximo mês", labelPrevious: () => "Ir para o mês anterior", labelMonthDropdown: () => "Escolher mês", labelYearDropdown: () => "Escolher ano",
      labelDayButton: (date: Date) => date.toLocaleDateString("pt-BR", { weekday: "long", day: "numeric", month: "long", year: "numeric" }) },
  };
  if (props.mode === "single") return <DayPicker {...common} mode="single" selected={toCalendarDate(props.value ?? "")} />;
  const range: DateRange | undefined = toCalendarDate(props.from ?? "") ? { from: toCalendarDate(props.from!), to: toCalendarDate(props.to ?? "") } : undefined;
  return <DayPicker {...common} mode="range" selected={range} />;
}
