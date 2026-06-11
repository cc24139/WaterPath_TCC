import type { LoginResponseDTO } from "@/api/dtos/userDTO";

export const authSessionChangedEvent = "water-path-auth-session-changed";

const tokenStorageKey = "token";
const userIdStorageKey = "userId";
const legacyUserIdStorageKey = "id";
const userEmailStorageKey = "userEmail";
const userNameStorageKey = "userName";
const tokenCookieName = "token";
const defaultCookieMaxAge = 60 * 60 * 24 * 7;

export interface AuthSession {
  token: string;
  id: string;
  name: string;
  email: string;
}

export function saveAuthSession(user: LoginResponseDTO) {
  if (!isBrowser()) {
    return;
  }

  const id = user.id.toString();

  localStorage.setItem(tokenStorageKey, user.token);
  localStorage.setItem(userIdStorageKey, id);
  localStorage.setItem(legacyUserIdStorageKey, id);
  localStorage.setItem(userEmailStorageKey, user.email);
  localStorage.setItem(userNameStorageKey, user.nome);

  setTokenCookie(user.token);
  notifyAuthSessionChange();
}

export function getAuthSession(): AuthSession | null {
  if (!isBrowser()) {
    return null;
  }

  const storedToken = localStorage.getItem(tokenStorageKey);
  const cookieToken = getCookie(tokenCookieName);
  const token = storedToken ?? cookieToken;

  if (!token) {
    return null;
  }

  if (isJwtExpired(token)) {
    clearAuthSession();
    return null;
  }

  if (storedToken && !cookieToken) {
    setTokenCookie(storedToken);
  }

  const id =
    localStorage.getItem(userIdStorageKey) ??
    localStorage.getItem(legacyUserIdStorageKey);
  const name = localStorage.getItem(userNameStorageKey);
  const email = localStorage.getItem(userEmailStorageKey);

  if (!id || !name || !email) {
    clearAuthSession();
    return null;
  }

  return {
    token,
    id,
    name,
    email,
  };
}

export function clearAuthSession() {
  if (!isBrowser()) {
    return;
  }

  localStorage.removeItem(tokenStorageKey);
  localStorage.removeItem(userIdStorageKey);
  localStorage.removeItem(legacyUserIdStorageKey);
  localStorage.removeItem(userEmailStorageKey);
  localStorage.removeItem(userNameStorageKey);

  document.cookie = `${tokenCookieName}=; Max-Age=0; Path=/; SameSite=Lax`;
  notifyAuthSessionChange();
}

function setTokenCookie(token: string) {
  const expiresAt = getJwtExpiresAt(token);
  const maxAge = expiresAt
    ? Math.max(0, Math.floor(expiresAt - Date.now() / 1000))
    : defaultCookieMaxAge;
  const secure = window.location.protocol === "https:" ? "; Secure" : "";

  document.cookie = `${tokenCookieName}=${encodeURIComponent(
    token
  )}; Max-Age=${maxAge}; Path=/; SameSite=Lax${secure}`;
}

function getCookie(name: string) {
  const cookie = document.cookie
    .split("; ")
    .find((item) => item.startsWith(`${name}=`));

  return cookie ? decodeURIComponent(cookie.split("=")[1]) : null;
}

function notifyAuthSessionChange() {
  window.dispatchEvent(new Event(authSessionChangedEvent));
}

function isJwtExpired(token: string) {
  const expiresAt = getJwtExpiresAt(token);

  return Boolean(expiresAt && expiresAt <= Date.now() / 1000);
}

function getJwtExpiresAt(token: string) {
  try {
    const [, payload] = token.split(".");

    if (!payload) {
      return null;
    }

    const normalizedPayload = normalizeBase64Url(payload);
    const decodedPayload = JSON.parse(window.atob(normalizedPayload)) as {
      exp?: unknown;
    };

    return typeof decodedPayload.exp === "number" ? decodedPayload.exp : null;
  } catch {
    return null;
  }
}

function isBrowser() {
  return typeof window !== "undefined";
}

function normalizeBase64Url(value: string) {
  const base64 = value.replace(/-/g, "+").replace(/_/g, "/");
  const padding = "=".repeat((4 - (base64.length % 4)) % 4);

  return `${base64}${padding}`;
}
