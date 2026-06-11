import { getAuthSession } from "@/features/auth/utils/authSession";

export const getAuthHeaders = (includeJsonContentType = false): HeadersInit => {
  const token = getAuthSession()?.token ?? null;

  const headers: Record<string, string> = {};

  if (includeJsonContentType) {
    headers["Content-Type"] = "application/json";
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return headers;
};
