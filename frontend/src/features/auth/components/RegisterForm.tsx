"use client";

import { useRegister } from "@/api/hooks/useUsers";
import { AuthFormCard } from "./AuthFormCard";
import { useState } from "react";

export function RegisterForm() {
  const { register, error } = useRegister();
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
    if (form.password !== form.confirmPassword) {
      alert("As senhas não coincidem. Por favor, tente novamente.");
      return;
    }
    const response = await register({
      nome: form.username,
      email: form.email,
      senha: form.password,
    });
    console.log(response)
    if(error){
      alert(error);
      return;
    }
      alert("Enviamos um codigo de confirmação para seu email! verique sua caixa de entrada para confirmar seu cadastro.");
      //Redirecionar para a página de confirmação de email
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
        { placeholder: "Informe o seu nome de usuário", name: "username", onChange: handleChange, value: form.username },
        { placeholder: "Informe o seu email", type: "email", name: "email", onChange: handleChange, value: form.email },
        {
          placeholder: "Informe a sua senha",
          type: "password",
          name: "password",
          onChange: handleChange,
          value: form.password
        },
        {
          placeholder: "Confirme a sua senha",
          type: "password",
          name: "confirmPassword",
          onChange: handleChange,
          value: form.confirmPassword
        },
      ]}
      onSecondaryAction={() => console.log("cancelou")}
      onPrimaryAction={handleRegister}
      primaryActionText="Cadastrar"
    />
  );
}
