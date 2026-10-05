import { notFound } from "next/navigation";
import { parseWaterBodyId } from "@/features/river-analysis/utils/waterBodyId";
import { WaterBodyDashboard } from "@/features/river-analysis/components/WaterBodyDashboard";

export default async function Page({ params }: { params: Promise<{ waterBodyId: string }> }) {
  const { waterBodyId } = await params;
  const id = parseWaterBodyId(waterBodyId);
  if (id === null) notFound();
  return <WaterBodyDashboard key={id} waterBodyId={id} />;
}
