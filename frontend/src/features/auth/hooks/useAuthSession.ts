"use client";

import { useEffect, useState } from "react";

import {
  authSessionChangedEvent,
  getAuthSession,
} from "@/features/auth/utils/authSession";

export interface AuthSessionUser {
  name: string;
  email: string;
}

export function useAuthSession() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [user, setUser] = useState<AuthSessionUser | null>(null);

  useEffect(() => {
    function syncAuthSession() {
      const session = getAuthSession();
      setIsAuthenticated(Boolean(session));

      if (!session) {
        setUser(null);
        return;
      }

      setUser({
        name: session.name,
        email: session.email,
      });
    }

    syncAuthSession();

    window.addEventListener("focus", syncAuthSession);
    window.addEventListener("storage", syncAuthSession);
    window.addEventListener(authSessionChangedEvent, syncAuthSession);

    return () => {
      window.removeEventListener("focus", syncAuthSession);
      window.removeEventListener("storage", syncAuthSession);
      window.removeEventListener(authSessionChangedEvent, syncAuthSession);
    };
  }, []);

  return { isAuthenticated, user };
}
