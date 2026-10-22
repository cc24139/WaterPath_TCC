import { AnalysisHistory } from "@/features/analysis-risk/components/AnalysisHistory";
import { notFound } from "next/navigation";
import { parseWaterBodyId } from "@/features/water-body-monitoring/utils/waterBodyId";

export default async function Page({ params }: { params: Promise<{ waterBodyId: string }> }) {
  const { waterBodyId } = await params;
  const id = parseWaterBodyId(waterBodyId);
  if (id === null) notFound();
  return <AnalysisHistory waterBodyId={id} />;
}
