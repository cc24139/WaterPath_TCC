import { AddAnalysisPage } from "@/features/add-analysis/components/AddAnalysisPage";

export default async function AddAnalysis({ searchParams }: { searchParams: Promise<{ coletaId?: string }> }) {
  const { coletaId } = await searchParams;
  const resumeCollectionId = coletaId && /^[1-9]\d*$/.test(coletaId) && Number(coletaId) <= 2147483647 ? Number(coletaId) : null;
  return <AddAnalysisPage resumeCollectionId={resumeCollectionId} />;
}
