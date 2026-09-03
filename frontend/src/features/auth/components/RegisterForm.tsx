"use client";

import { useRegister } from "@/api/hooks/useUsers";
import { AuthFormCard } from "./AuthFormCard";
import { useState } from "react";
import { useRouter } from "next/navigation";

export function RegisterForm() {
  const { register, loading } = useRegister();
  const router = useRouter();
  const [form, setForm] = useState({
    username: "",
    email: "",
    password: "",
    confirmPassword: "",
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleRegister = async () => {
    if (!form.username.trim() || !form.email.trim() || !form.password) {
      alert("Preencha todos os campos obrigatórios.");
      return;
    }

    if (form.password !== form.confirmPassword) {
      alert("As senhas não coincidem. Por favor, tente novamente.");
      return;
    }

    const result = await register({
      nome: form.username.trim(),
      email: form.email.trim(),
      senha: form.password,
    });

    if (!result.ok) {
      alert(result.message);
      return;
    }

    alert("Conta criada com sucesso. Agora você já pode entrar.");
    router.replace("/login");
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
      subtitle="Informações para o Cadastro:"
      fields={[
        {
          placeholder: "Informe o seu nome de usuário",
          name: "username",
          autoComplete: "name",
          required: true,
          onChange: handleChange,
          value: form.username,
        },
        {
          placeholder: "Informe o seu email",
          type: "email",
          name: "email",
          autoComplete: "email",
          required: true,
          onChange: handleChange,
          value: form.email,
        },
        {
          placeholder: "Informe a sua senha",
          type: "password",
          name: "password",
          autoComplete: "new-password",
          required: true,
          onChange: handleChange,
          value: form.password,
        },
        {
          placeholder: "Confirme a sua senha",
          type: "password",
          name: "confirmPassword",
          autoComplete: "new-password",
          required: true,
          onChange: handleChange,
          value: form.confirmPassword,
        },
      ]}
      onSecondaryAction={handleCancel}
      onPrimaryAction={handleRegister}
      primaryActionText={loading ? "Cadastrando..." : "Cadastrar"}
      isSubmitting={loading}
    />
  );
}
