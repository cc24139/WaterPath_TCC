import { InputField } from "@/features/auth/components/InputField";
import { Button } from "@/features/auth/components/Button";
import { Header} from "@/components/layout/Header";
import { RiverSearchBar } from "@/features/search/components/RiverSearchBar";

export default function Home() {
  return (
    <>
    
    <Header/>
    <InputField placeholder="Informe seu nome de usuário" />
    <Button text="Entrar" variant="primary" />
    <Button text="Cancelar" variant="secondary" />
    <RiverSearchBar />
    </>
  );
}
