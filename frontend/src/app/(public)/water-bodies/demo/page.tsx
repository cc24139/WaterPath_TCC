import { WaterBodiesView } from "@/features/search/components/WaterBodiesView";
import { demoRivers } from "@/features/search/constants/demoRivers";

export const metadata = {
  title: "Prévia dos corpos hídricos | Water Path",
  robots: { index: false, follow: false },
};

export default function WaterBodiesDemo() {
  return <WaterBodiesView rivers={demoRivers} demo />;
}
