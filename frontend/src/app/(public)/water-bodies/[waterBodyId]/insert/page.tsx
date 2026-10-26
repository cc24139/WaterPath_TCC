import { notFound } from "next/navigation";
import { parseWaterBodyId } from "@/features/water-body-monitoring/utils/waterBodyId";
import { AddAnalysisPage } from "@/features/add-analysis/components/AddAnalysisPage";

export default async function Page({ params }: { params: Promise<{ waterBodyId: string }> }) {
  const { waterBodyId } = await params;
  const id = parseWaterBodyId(waterBodyId);
  if (id === null) notFound();
  return <AddAnalysisPage key={id} waterBodyId={id} />;
}
