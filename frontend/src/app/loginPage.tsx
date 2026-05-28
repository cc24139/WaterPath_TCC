import { InputField } from "@/features/auth/components/InputField"
import { Button } from "@/features/auth/components/Button";


type LoginProps = {
    token: string;
    email: string;
    password: string;
}

function LoginPage() {
    return (
      <div className="flex justify-between">
        <div className="flex h-screen w-full items-center justify-center bg-blue-300">
          <p>Component</p>
        </div>
        <div className="flex bg-gray-300  h-screen w-full items-center justify-center text-center text-white">
          <div className=" h-1/2 shadow-md w-1/2 flex-col items-center justify-center rounded-lg bg-white">
                <h1 className="text-black p-4 font-semibold">Dê o ultimo passo para nos ajudar!</h1>
                <h3 className="text-black">Informações para o Login</h3>
                <InputField placeholder="Insira seu email" />
                <InputField placeholder="Insira sua senha" />
                <p className="text-gray-700">Esqueceu sua senha? <a href="#" className="text-secondary">Recupere</a></p>
                <Button text="Entrar" variant="primary"></Button>
          </div>
        </div>
      </div>
    );
}

export default LoginPage;