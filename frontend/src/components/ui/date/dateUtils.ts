/** Datas de calendário não são instantes UTC. Conversão para ISO ocorre só no envio da coleta. */
export function toCalendarDate(value: string): Date | undefined {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (!match) return undefined;
  const [, year, month, day] = match.map(Number);
  if (year < 1900 || year > 9999) return undefined;
  const date = new Date(year, month - 1, day, 12);
  return date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day ? date : undefined;
}
export function calendarISO(date: Date): string {
  return `${date.getFullYear().toString().padStart(4, "0")}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;
}
export function displayDate(iso: string): string {
  if (!toCalendarDate(iso)) return "";
  const [year, month, day] = iso.split("-");
  return `${day}/${month}/${year}`;
}
export function parseDisplayDate(text: string): string | undefined {
  const match = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(text.trim());
  if (!match) return undefined;
  const iso = `${match[3]}-${match[2]}-${match[1]}`;
  return toCalendarDate(iso) ? iso : undefined;
}
export function dateInputText(value: string): string {
  if (toCalendarDate(value)) return displayDate(value);
  const digits = value.replace(/\D/g, "").slice(0, 8);
  return [digits.slice(0, 2), digits.slice(2, 4), digits.slice(4)].filter(Boolean).join("/");
}
export function calendarToday(timeZone?: string, now = new Date()): string {
  return timeZone ? now.toLocaleDateString("sv-SE", { timeZone }) : calendarISO(now);
}
export function shiftDate(iso: string, days: number): string {
  const date = toCalendarDate(iso);
  if (!date) throw new Error("Data inválida");
  date.setDate(date.getDate() + days);
  return calendarISO(date);
}
export function rangeDays(from: string, to: string): number {
  const start = toCalendarDate(from), end = toCalendarDate(to);
  if (!start || !end) return NaN;
  return Math.round((Date.UTC(end.getFullYear(), end.getMonth(), end.getDate()) - Date.UTC(start.getFullYear(), start.getMonth(), start.getDate())) / 86400000) + 1;
}
export function dateError(value: string, min = "1900-01-01", max?: string): string | null {
  if (!toCalendarDate(value)) return "Informe uma data válida no formato DD/MM/AAAA.";
  if (value < min) return `Escolha uma data a partir de ${displayDate(min)}.`;
  if (max && value > max) return `Escolha uma data até ${displayDate(max)}.`;
  return null;
}
export function rangeError(from: string, to: string, min: string, max: string, maxDays = 366): string | null {
  const invalid = dateError(from, min, max) || dateError(to, min, max);
  if (invalid) return invalid;
  if (from > to) return "A data inicial deve ser anterior ou igual à data final.";
  if (rangeDays(from, to) > maxDays) return `Selecione um período de até ${maxDays} dias.`;
  return null;
}
export function periodPresets(today: string): { label: string; from: string; to: string }[] {
  const monthStart = `${today.slice(0, 7)}-01`;
  const lastMonthEnd = shiftDate(monthStart, -1);
  return [
    { label: "Últimos 7 dias", from: shiftDate(today, -6), to: today },
    { label: "Últimos 30 dias", from: shiftDate(today, -29), to: today },
    { label: "Este mês", from: monthStart, to: today },
    { label: "Mês anterior", from: `${lastMonthEnd.slice(0, 7)}-01`, to: lastMonthEnd },
  ];
}
export function normalizeTime(text: string): string | undefined {
  const match = /^(\d{1,2}):(\d{2})$/.exec(text.trim());
  return match && Number(match[1]) <= 23 && Number(match[2]) <= 59 ? `${match[1].padStart(2, "0")}:${match[2]}` : undefined;
}
export function timeInputText(raw: string): string {
  const text = raw.replace(/[^\d:]/g, "").slice(0, 5);
  if (text.includes(":")) return normalizeTime(text) ?? text;
  const digits = text.slice(0, 4);
  return digits.length > 2 ? `${digits.slice(0, 2)}:${digits.slice(2)}` : digits;
}
