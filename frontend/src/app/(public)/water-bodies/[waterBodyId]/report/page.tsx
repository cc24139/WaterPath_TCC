import { notFound } from "next/navigation";
import Report from "@/features/river-analysis/components/WaterBodyReport";
import { parseWaterBodyId } from "@/features/river-analysis/utils/waterBodyId";
export default async function ReportPage({ params }: { params: Promise<{ waterBodyId: string }> }) {
  const { waterBodyId } = await params;
  const id = parseWaterBodyId(waterBodyId);
  if (id === null) notFound();
  return <Report key={id} />;
}
