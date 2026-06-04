"use client";

import { useState } from "react";
import { AuthFormCard } from "./AuthFormCard";
import { useLogin } from "@/api/hooks/useUsers";
import { LoginResponseDTO } from "@/api/dtos/userDTO";

export function LoginForm() {
  const { login, loading, error } = useLogin();
  const [form, setForm] = useState({
    email: "",
    senha: "",
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleLogin = async () => {
    const response: LoginResponseDTO | undefined = await login(form);
    console.log("Resposta do login:", response);
    if (response) {
      console.log("Login bem-sucedido:", response);
      // Aqui você pode redirecionar o usuário ou armazenar o token, etc.
      localStorage.setItem("token", response.token);
      alert("Login bem-sucedido!");
    } else {
      console.error("Erro no login: resposta vazia");
      alert("Usuario e senha invalidos");
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
