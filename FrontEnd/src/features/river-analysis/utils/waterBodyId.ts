export function parseWaterBodyId(value: string): number | null {
  if (!/^[1-9]\d*$/.test(value)) return null;
  const id = Number(value);
  return Number.isInteger(id) && id <= 2147483647 ? id : null;
}
