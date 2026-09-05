"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { AuthFormCard } from "./AuthFormCard";
import { useLogin } from "@/api/hooks/useUsers";
import { LoginResponseDTO } from "@/api/dtos/userDTO";
import { saveAuthSession } from "@/features/auth/utils/authSession";

export function LoginForm({ registered = false }: { registered?: boolean }) {
  const { login, loading } = useLogin();
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [hasAttemptedLogin, setHasAttemptedLogin] = useState(false);
  const [form, setForm] = useState({
    email: "",
    senha: "",
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleLogin = async () => {
    setError(null);
    setHasAttemptedLogin(true);

    try {
      const response = await login({
        email: form.email.trim(),
        senha: form.senha,
      });

      if (response instanceof Response) {
        setError(getLoginErrorMessage(response.status));
        return;
      }

      if (!isLoginResponse(response)) {
        setError("Falha no login. Tente novamente.");
        return;
      }

      saveAuthSession(response);
      router.replace(getSafeRedirectPath());
    } catch {
      setError("Não foi possível conectar ao servidor. Tente novamente.");
      return;
    }
  };

  const handleCancel = () => {
    router.push("/");
  };

  return (
    <AuthFormCard
      title={
        <>
          Dê o último passo
          <br />
          para nos ajudar!
        </>
      }
      subtitle="Informações para o Login:"
      fields={[
        {
          placeholder: "Insira seu email",
          type: "email",
          name: "email",
          autoComplete: "email",
          required: true,
          onChange: handleChange,
          value: form.email,
        },
        {
          placeholder: "Insira sua senha",
          type: "password",
          name: "senha",
          autoComplete: "current-password",
          required: true,
          onChange: handleChange,
          value: form.senha,
        },
      ]}
      recoveryLink={
        <>
          Esqueceu sua senha?{" "}
          <a href="#" className="text-secondary">
            Recupere
          </a>
        </>
      }
      onSecondaryAction={handleCancel}
      onPrimaryAction={handleLogin}
      primaryActionText={loading ? "Entrando..." : "Entrar"}
      isSubmitting={loading}
      error={error}
      successMessage={
        registered && !hasAttemptedLogin
          ? "Conta criada com sucesso. Agora você já pode entrar."
          : null
      }
    />
  );

}

function isLoginResponse(response: unknown): response is LoginResponseDTO {
  if (!response || typeof response !== "object") {
    return false;
  }

  const data = response as Partial<LoginResponseDTO>;

  return (
    typeof data.id === "number" &&
    typeof data.token === "string" &&
    typeof data.email === "string" &&
    typeof data.nome === "string"
  );
}

function getLoginErrorMessage(status: number) {
  if (status === 403) {
    return "Email não confirmado. Por favor, verifique seu email para confirmar sua conta.";
  }

  if (status === 401) {
    return "Email ou senha inválidos.";
  }

  return "Não foi possível entrar. Tente novamente.";
}

function getSafeRedirectPath() {
  if (typeof window === "undefined") {
    return "/";
  }

  const requestedPath = new URLSearchParams(window.location.search).get("next");

  if (
    requestedPath?.startsWith("/") &&
    !requestedPath.startsWith("//")
  ) {
    return requestedPath;
  }

  return "/";
}
