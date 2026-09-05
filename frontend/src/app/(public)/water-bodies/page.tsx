"use client";

import { WaterBodiesView } from "@/features/search/components/WaterBodiesView";
import { useWaterBodies } from "@/features/search/hooks/useWaterBodies";

export default function WaterBodies() {
  const state = useWaterBodies();
  return <WaterBodiesView {...state} />;
}
