"use client";
import { InputField } from "@/features/auth/components/InputField";
import { Button } from "@/features/auth/components/Button";
import { Header} from "@/components/layout/Header";
import { RiverListToolBar } from "@/features/search/components/RiverListToolBar";
import { RiverConditionCards } from "@/features/river-analysis/components/RiverConditionCards";
import { RiverInsightCards } from "@/features/river-analysis/components/RiverInsightCards";
import { SideBar} from "@/components/layout/SideBar";
import LoginPage from "./loginPage";
import {inter,comicNeue} from "./fonts";
import ButtonComponent from "@/components/ui/Button";
export default function Home() {

  return (
      <ButtonComponent onClick={() => console.log("Button clicked")} 
      className={`bg-white text-contrast px-4 py-2 rounded ${inter.className}`}>
        Cadastrar
      </ButtonComponent>
  );
}
