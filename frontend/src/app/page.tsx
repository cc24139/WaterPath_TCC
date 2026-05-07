import { InputField } from "@/features/auth/components/InputField";
import { Button } from "@/features/auth/components/Button";
import { Header} from "@/components/layout/Header";
import { RiverListToolBar } from "@/features/search/components/RiverListToolBar";
import { RiverConditionCards } from "@/features/river-analysis/components/RiverConditionCards";

export default function Home() {
  return (
    <>
    
    <Header/>
    <InputField placeholder="Informe seu nome de usuário" />
    <Button text="Entrar" variant="primary" />
    <Button text="Cancelar" variant="secondary" />
    <RiverListToolBar />
    <RiverConditionCards />
    </>
  );
}
