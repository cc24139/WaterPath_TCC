export function canAnalyzeWaterBody(value: unknown, userId: string): boolean {
  if (!value || typeof value !== "object" || Array.isArray(value)) return false;
  const body = value as Record<string, unknown>;
  if (body.ehPrivado === false) return true;
  return Array.isArray(body.users) && body.users.some((member) =>
    member && typeof member === "object" && String((member as Record<string, unknown>).id) === userId);
}
