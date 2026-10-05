import { notFound } from "next/navigation";
import Report from "@/features/report/components/WaterBodyReport";
import { parseWaterBodyId } from "@/features/water-body-monitoring/utils/waterBodyId";
export default async function ReportPage({ params }: { params: Promise<{ waterBodyId: string }> }) {
  const { waterBodyId } = await params;
  const id = parseWaterBodyId(waterBodyId);
  if (id === null) notFound();
  return <Report key={id} />;
}
