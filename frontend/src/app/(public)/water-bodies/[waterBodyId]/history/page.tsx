import { WaterBodyHistory } from "@/features/water-body-monitoring/history/components/WaterBodyHistory";
import { notFound } from "next/navigation";
import { parseWaterBodyId } from "@/features/water-body-monitoring/utils/waterBodyId";

export default async function Page({ params }: { params: Promise<{ waterBodyId: string }> }) {
  const { waterBodyId } = await params;
  const id = parseWaterBodyId(waterBodyId);
  if (id === null) notFound();
  return <WaterBodyHistory waterBodyId={id} />;
}
