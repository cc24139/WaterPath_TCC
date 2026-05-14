"use client";

import { useState } from "react";
import {
  LuChartNoAxesCombined,
  LuClock3,
  LuFileText,
  LuLayoutDashboard,
  LuLogOut,
  LuMenu,
  LuSquarePen,
  LuX,
} from "react-icons/lu";

import { Logo } from "@/components/ui/Logo";

const activeLink = "/dashboard";

const sidebarLinks = [
  {
    label: "Dashboard",
    href: "/dashboard",
    function: "view",
    icon: LuLayoutDashboard,
  },
  {
    label: "Histórico",
    href: "/history",
    function: "view",
    icon: LuClock3,
  },
  {
    label: "Análise",
    href: "/analysis",
    function: "view",
    icon: LuChartNoAxesCombined,
  },
  {
    label: "Relatório",
    href: "/report",
    function: "view",
    icon: LuFileText,
  },
  {
    label: "Inserir Dados",
    href: "/insert",
    function: "action",
    icon: LuSquarePen,
  },
  {
    label: "Sair",
    href: "/rivers",
    function: "action",
    icon: LuLogOut,
  },
];

const viewLinks = sidebarLinks.filter((link) => link.function === "view");
const actionLinks = sidebarLinks.filter((link) => link.function === "action");

export function SideBar() {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <>
      <header className="relative flex h-20 w-full items-center border-b border-placeholder bg-background px-8 shadow-[0_8px_15px_rgba(23,166,191,0.18)] lg:hidden">
        <button
          type="button"
          aria-label="Abrir menu"
          aria-expanded={isOpen}
          onClick={() => setIsOpen(true)}
          className="inline-flex h-11 w-11 items-center justify-center text-primary transition-colors hover:text-secondary"
        >
          <LuMenu className="h-7 w-7" />
        </button>

        <div className="absolute left-1/2 flex -translate-x-1/2 items-center gap-2">
          <Logo className="h-9 w-auto" />
          <h1 className="font-heading text-[24px] font-bold text-primary drop-shadow-[0_3px_3px_rgba(23,166,191,0.35)] sm:text-[28px]">
            Water Path
          </h1>
        </div>
      </header>

      {isOpen && (
        <button
          type="button"
          aria-label="Fechar menu"
          onClick={() => setIsOpen(false)}
          className="fixed inset-0 z-40 bg-black/35 lg:hidden"
        />
      )}

      <aside
        className={`
          fixed inset-y-0 left-0 z-50 min-h-screen w-[230px] border-r border-placeholder
          bg-background px-5 py-4 transition-transform duration-300 lg:static lg:z-auto
          lg:w-[210px] lg:translate-x-0
          ${isOpen ? "translate-x-0" : "-translate-x-full"}
        `}
      >
        <button
          type="button"
          aria-label="Fechar menu"
          onClick={() => setIsOpen(false)}
          className="absolute right-3 top-3 inline-flex h-9 w-9 items-center justify-center text-text-secondary transition-colors hover:text-primary lg:hidden"
        >
          <LuX className="h-6 w-6" />
        </button>

        <SideBarContent onNavigate={() => setIsOpen(false)} />
      </aside>
    </>
  );
}

function SideBarContent({ onNavigate }: { onNavigate?: () => void }) {
  return (
    <nav className="flex h-full flex-col gap-10">
      <section className="flex flex-col gap-2">
        <div className="flex items-center gap-2">
          <Logo className="h-8 w-auto" />
          <h1 className="font-heading text-[18px] font-bold text-primary">
            Water Path
          </h1>
        </div>

        <p className="font-heading text-[11px] text-text-secondary">
          Painel de Monitoramento
        </p>
      </section>

      <section className="flex flex-col gap-11">
        <div className="flex flex-col gap-4">
          <h2 className="font-primary font-medium text-[12px] text-text-secondary">
            Visualização
          </h2>

          <ul className="flex flex-col gap-5">
            {viewLinks.map((link) => {
              const Icon = link.icon;
              const isActive = link.href === activeLink;

              return (
                <li key={link.href}>
                  <a
                    href={link.href}
                    onClick={onNavigate}
                    className={`
                        relative flex items-center gap-3 rounded-md py-1 pl-4
                        font-heading text-[13px]
                        ${isActive ? "text-secondary" : "text-text-primary"}
                      `}
                  >
                    {isActive && (
                      <span className="absolute left-0 h-9 w-1.5 rounded-full bg-secondary" />
                    )}

                    <Icon className="h-5 w-5 shrink-0" />
                    <span>{link.label}</span>
                  </a>
                </li>
              );
            })}
          </ul>
        </div>

        <div className="flex flex-col gap-4">
          <h2 className="font-primary font-medium text-[12px] text-text-secondary">
            Ações
          </h2>

          <ul className="flex flex-col gap-5">
            {actionLinks.map((link) => {
              const Icon = link.icon;

              return (
                <li key={link.href}>
                  <a
                    href={link.href}
                    onClick={onNavigate}
                    className="
                        flex items-center gap-3 rounded-md py-1 pl-4
                        font-heading text-[13px] text-text-primary
                      "
                  >
                    <Icon className="h-5 w-5 shrink-0" />
                    <span>{link.label}</span>
                  </a>
                </li>
              );
            })}
          </ul>
        </div>
      </section>
    </nav>
  );
}
