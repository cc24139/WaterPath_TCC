"use client";

import { AuthFormCard } from "./AuthFormCard";



export function LoginForm() {
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
      onPrimaryAction={() => console.log("entrou")}
      primaryActionText="Entrar"
    />
  );
}


