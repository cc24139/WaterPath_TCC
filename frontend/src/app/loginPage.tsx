import { InputField } from "@/features/auth/components/InputField"
import { Button } from "@/features/auth/components/Button";
import ButtonComponent from "@/components/ui/Button";
import LogoParabola from "@/components/ui/logoParabola";


type LoginProps = {
    token: string;
    email: string;
    password: string;
}

function LoginPage() {
    return (
      <div className="flex justify-between items-center bg-gray-300" >
        <div className="flex h-screen w-full items-center ">
          <LogoParabola />
        </div>
        <div className="flex flex-col bg-gray-300  h-screen w-full items-center justify-center text-center text-white pr-32">
          <div className="shadow-md w-10/12  items-center h-1/2 justify-center rounded-lg bg-white ">
            <h1 className="text-black p-4 font-semibold text-2xl  ">
              Dê o ultimo passo para nos ajudar!
            </h1>
            <h3 className="text-black pb-10">Informações para o Login</h3>
            <InputField placeholder="Insira seu email" type="email" />
            <InputField placeholder="Insira sua senha" type="password" />
            <p className="text-gray-700">
              Esqueceu sua senha?{" "}
              <a href="#" className="text-secondary">
                Recupere
              </a>
            </p>
            <div className="mt-4 flex items-center justify-center">
              <ButtonComponent
                onClick={() => console.log("entrou")}
                className="bg-white hover:bg-blue-700 text-contrast font-bold py-2 px-4 rounded shadow-md mr-4"
              >
                Cancelar
              </ButtonComponent>
              <ButtonComponent
                onClick={() => console.log("entrou")}
                className="bg-contrast hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
              >
                Entrar
              </ButtonComponent>
            </div>
          </div>
        </div>
      </div>
    );
}

export default LoginPage;