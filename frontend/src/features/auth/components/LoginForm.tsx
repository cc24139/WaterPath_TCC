"use client";

import { useState } from "react";
import { AuthFormCard } from "./AuthFormCard";
import { useLogin } from "@/api/hooks/useUsers";
import { LoginResponseDTO } from "@/api/dtos/userDTO";
import { saveAuthSession } from "@/features/auth/utils/authSession";

export function LoginForm() {
  const { login } = useLogin();
  const [form, setForm] = useState({
    email: "",
    senha: "",
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleLogin = async () => {
    try {
      const response = await login(form);

      if (response instanceof Response) {
        alert(getLoginErrorMessage(response.status));
        return;
      }

      if (!isLoginResponse(response)) {
        alert("Falha no login. Tente novamente.");
        return;
      }

      saveAuthSession(response);
      alert("Login bem-sucedido! Redirecionando para a página inicial...");
      window.location.href = "/";
    } catch {
      alert("Não foi possível conectar ao servidor. Tente novamente.");
      return;
    }
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
          onChange: handleChange,
          value: form.email,
        },
        {
          placeholder: "Insira sua senha",
          type: "password",
          name: "senha",
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
      onSecondaryAction={() => console.log("cancelou")}
      onPrimaryAction={handleLogin}
      primaryActionText="Entrar"
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
  if (status === 401) {
    return "Email não confirmado. Por favor, verifique seu email para confirmar sua conta.";
  }

  return "Falha no login. Verifique suas credenciais e tente novamente.";
}
