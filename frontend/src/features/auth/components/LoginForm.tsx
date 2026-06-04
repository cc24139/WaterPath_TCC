"use client";

import { AuthFormCard } from "./AuthFormCard";
import { useLogin } from "@/api/hooks/useUsers";



export function LoginForm() {
  const { state, login } = useLogin("", "");

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
        { placeholder: "Insira seu email", type: "email", name: "email" },
        { placeholder: "Insira sua senha", type: "password", name: "password" },
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
      onPrimaryAction={() => login().then(() => console.log("logou"))}
      primaryActionText="Entrar"
    />
  );
}


