import { AuthPageLayout } from "@/features/auth/components/AuthPageLayout";
import { LoginForm } from "@/features/auth/components/LoginForm";

export default function Login() {
  return (
    <AuthPageLayout>
      <LoginForm />
    </AuthPageLayout>
  );
}
