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
    const response = await login(form);
    if(error){
      alert(error);
      return;
    }
    console.log("Resposta do login no componente:", response);
    const dto = response as unknown as LoginResponseDTO;
    localStorage.setItem("token", dto.token);
    localStorage.setItem("userId", dto.id.toString());
    localStorage.setItem("userEmail", dto.email);
    localStorage.setItem("userName", dto.nome);
    alert("Login bem-sucedido! Redirecionando para a página inicial...");
    window.location.href = "/home";
    }

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