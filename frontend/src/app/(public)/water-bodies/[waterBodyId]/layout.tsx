import type { ReactNode } from "react";
import { MonitoringSidebar } from "@/features/water-body-monitoring/components/MonitoringSidebar";

export default async function WaterBodyLayout({ children, params }: {
  children: ReactNode;
  params: Promise<{ waterBodyId: string }>;
}) {
  const { waterBodyId } = await params;
  return (
    <div className="min-h-dvh bg-background text-text-primary lg:flex">
      <MonitoringSidebar waterBodyId={waterBodyId} />
      <main id="monitoring-content" className="min-w-0 flex-1 px-4 py-6 sm:px-6 lg:px-8">
        <div className="mx-auto w-full max-w-[1440px]">{children}</div>
      </main>
    </div>
  );
}
