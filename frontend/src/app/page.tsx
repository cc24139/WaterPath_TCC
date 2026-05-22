import { InputField } from "@/features/auth/components/InputField";
import { Button } from "@/features/auth/components/Button";
import { Header} from "@/components/layout/Header";
import { RiverListToolBar } from "@/features/search/components/RiverListToolBar";
import { RiverConditionCards } from "@/features/river-analysis/components/RiverConditionCards";
import { RiverInsightCards } from "@/features/river-analysis/components/RiverInsightCards";
import { SideBar} from "@/components/layout/SideBar";
import LoginPage from "./loginPage";
export default function Home() {
  return (
      <LoginPage />
  );
}
