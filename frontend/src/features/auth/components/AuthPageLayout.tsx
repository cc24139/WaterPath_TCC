import type { ReactNode } from "react";

import { LogoParabola } from "@/components/ui/logoParabola";

interface AuthPageLayoutProps {
  children: ReactNode;
}

export function AuthPageLayout({ children }: AuthPageLayoutProps) {
  return (
    <main className="flex min-h-screen flex-col bg-gray-300 md:flex-row md:items-stretch md:overflow-hidden">
      <aside className="flex w-full justify-center md:min-h-screen md:w-[37vw] md:min-w-[284px] md:max-w-[500px] md:flex-none md:justify-start">
        <LogoParabola />
      </aside>

      <section className="flex min-h-[calc(100vh-360px)] w-full flex-1 items-center justify-center px-5 py-8 text-center text-white sm:px-8 md:min-h-screen md:min-w-0 md:px-[clamp(1.5rem,5vw,5rem)] lg:px-[clamp(2rem,8vw,8rem)]">
        {children}
      </section>
    </main>
  );
}
