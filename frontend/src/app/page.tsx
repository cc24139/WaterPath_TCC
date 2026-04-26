import { InputField } from "@/features/auth/components/InputField";
import { Button } from "@/features/auth/components/Button";

export default function Home() {
  return (
    <>
    <InputField placeholder="Informe seu nome de usuário" />
    <Button text="Entrar" variant="primary" />
    <Button text="Cancelar" variant="secondary" />
    </>
  );
}
