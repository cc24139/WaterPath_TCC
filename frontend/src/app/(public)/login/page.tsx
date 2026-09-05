import { AuthPageLayout } from "@/features/auth/components/AuthPageLayout";
import { LoginForm } from "@/features/auth/components/LoginForm";



export default async function Login({
  searchParams,
}: {
  searchParams: Promise<{ registered?: string | string[] }>;
}) {
  const { registered } = await searchParams;

  return (
    <AuthPageLayout>
      <LoginForm registered={registered === "true"} />
    </AuthPageLayout>
  );
}
