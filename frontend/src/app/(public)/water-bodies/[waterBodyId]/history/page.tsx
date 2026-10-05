import { notFound } from "next/navigation";
import { parseWaterBodyId } from "@/features/river-analysis/utils/waterBodyId";

export default async function Page({ params }: { params: Promise<{ waterBodyId: string }> }) {
  const { waterBodyId } = await params;
  const id = parseWaterBodyId(waterBodyId);
  if (id === null) notFound();
  return <h1 className="font-heading text-xl font-bold">Hist?rico</h1>;
}
