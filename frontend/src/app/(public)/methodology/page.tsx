import type { Metadata } from "next";

import { MethodologyPage } from "@/features/methodology/components/MethodologyPage";

export const metadata: Metadata = {
  title: "Metodologias | Water Path",
  description: "Entenda como o WaterPath utiliza medições da água, estimativas de metais e visão computacional para acompanhar os corpos hídricos.",
};

export default function Methodology() {
  return <MethodologyPage />;
}
