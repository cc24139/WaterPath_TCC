import { SideBar } from "@/components/layout/SideBar";
import { AnalysisHistory } from "@/features/analysis-risk/components/AnalysisHistory";

export function MyAnalysesPage() {
  return <div className="min-h-screen bg-background lg:flex">
    <SideBar variant="analysis-registration" subtitle="Área de cadastro" activeHref="/my-analyses" />
    <main className="min-w-0 flex-1 px-4 py-5 sm:px-6 lg:px-8 xl:px-10">
      <div className="mx-auto w-full max-w-[1500px]"><AnalysisHistory /></div>
    </main>
  </div>;
}
