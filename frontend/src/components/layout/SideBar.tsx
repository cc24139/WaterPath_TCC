
import {
  LuChartNoAxesCombined,
  LuClock3,
  LuFileText,
  LuLayoutDashboard,
  LuLogOut,
  LuSquarePen,
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
  return (
    <aside className="min-h-screen w-[190px] bg-background px-3 py-4">
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
    </aside>
  );
}
