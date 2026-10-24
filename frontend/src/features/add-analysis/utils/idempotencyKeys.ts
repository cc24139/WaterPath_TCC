type PendingRequest = { fingerprint: string; key: string };

export function requestKey(scope: string, fingerprint: string): string {
  const storageKey = `waterpath:pending:${scope}`;
  try {
    const pending = JSON.parse(localStorage.getItem(storageKey) ?? "null") as PendingRequest | null;
    if (pending?.fingerprint === fingerprint && /^[0-9a-f-]{36}$/i.test(pending.key)) return pending.key;
  } catch { /* Storage may be unavailable; the server still accepts this request. */ }
  const key = crypto.randomUUID();
  try { localStorage.setItem(storageKey, JSON.stringify({ fingerprint, key })); } catch { /* See above. */ }
  return key;
}

export function clearRequestKey(scope: string, key: string): void {
  const storageKey = `waterpath:pending:${scope}`;
  try {
    const pending = JSON.parse(localStorage.getItem(storageKey) ?? "null") as PendingRequest | null;
    if (pending?.key === key) localStorage.removeItem(storageKey);
  } catch { /* Storage is optional. */ }
}

export async function imageFingerprint(image: File, sample: unknown): Promise<string> {
  const digest = await crypto.subtle.digest("SHA-256", await image.arrayBuffer());
  const hex = Array.from(new Uint8Array(digest), byte => byte.toString(16).padStart(2, "0")).join("");
  return `${JSON.stringify(sample)}:${hex}`;
}
