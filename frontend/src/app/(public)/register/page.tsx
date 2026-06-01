import { AuthPageLayout } from "@/features/auth/components/AuthPageLayout";
import { RegisterForm } from "@/features/auth/components/RegisterForm";

export default function Register() {
  return (
    <AuthPageLayout>
      <RegisterForm />
    </AuthPageLayout>
  );
}
