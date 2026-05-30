import { AuthFormCard } from "@/features/auth/components/AuthFormCard";
import { AuthPageLayout } from "@/features/auth/components/AuthPageLayout";

export function RegisterPage() {
    return (
        <AuthPageLayout>
            <AuthFormCard
                title={(
                    <>
                        Dê o último passo
                        <br />
                        para nos ajudar!
                    </>
                )}
                subtitle="Informações para o Cadastro:"
                fields={[
                    { placeholder: "Informe o seu nome de usuário", name: "username" },
                    { placeholder: "Informe o seu email", type: "email", name: "email" },
                    { placeholder: "Informe a sua senha", type: "password", name: "password" },
                    { placeholder: "Confirme a sua senha", type: "password", name: "confirmPassword" },
                ]}
                onSecondaryAction={() => console.log("cancelou")}
                onPrimaryAction={() => console.log("cadastrou")}
                primaryActionText="Cadastrar"
            />
        </AuthPageLayout>
    );
}
