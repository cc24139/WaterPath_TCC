"use client";

import { useRegister } from "@/api/hooks/useUsers";
import { AuthFormCard } from "./AuthFormCard";
import { useState } from "react";
import { useRouter } from "next/navigation";

export function RegisterForm() {
  const { register, loading } = useRegister();
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
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
    setError(null);

    if (!form.username.trim() || !form.email.trim() || !form.password || !form.confirmPassword) {
      setError("Preencha todos os campos obrigatórios.");
      return;
    }

    if (form.password !== form.confirmPassword) {
      setError("As senhas não coincidem. Por favor, tente novamente.");
      return;
    }

    const result = await register({
      nome: form.username.trim(),
      email: form.email.trim(),
      senha: form.password,
    });

    if (!result.ok) {
      setError(result.message);
      return;
    }

    router.replace("/login?registered=true");
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
      error={error}
    />
  );
}
