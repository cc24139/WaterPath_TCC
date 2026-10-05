import { notFound, redirect } from "next/navigation";
import { parseWaterBodyId } from "@/features/river-analysis/utils/waterBodyId";
export default async function WaterBodyPage({ params }: { params: Promise<{ waterBodyId: string }> }) {
  const { waterBodyId } = await params;
  const id = parseWaterBodyId(waterBodyId);
  if (id === null) notFound();
  redirect(`/water-bodies/${id}/dashboard`);
}
