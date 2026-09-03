import {
  clearAuthSession,
  getAuthSession,
} from "@/features/auth/utils/authSession";

interface ApiFetchOptions {
  authenticated?: boolean;
  redirectOnUnauthorized?: boolean;
}

export async function apiFetch(
  input: RequestInfo | URL,
  init: RequestInit = {},
  options: ApiFetchOptions = {}
): Promise<Response> {
  const {
    authenticated = true,
    redirectOnUnauthorized = true,
  } = options;
  const headers = new Headers(init.headers);

  if (authenticated) {
    const token = getAuthSession()?.token;

    if (token) {
      headers.set("Authorization", `Bearer ${token}`);
    }
  }

  const response = await fetch(input, {
    ...init,
    headers,
  });

  if (
    authenticated &&
    redirectOnUnauthorized &&
    response.status === 401
  ) {
    handleUnauthorizedResponse();
  }

  return response;
}

function handleUnauthorizedResponse() {
  if (typeof window === "undefined") {
    return;
  }

  const currentPath = `${window.location.pathname}${window.location.search}`;

  clearAuthSession();

  if (window.location.pathname !== "/login") {
    window.location.replace(`/login?next=${encodeURIComponent(currentPath)}`);
  }
}
