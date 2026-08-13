"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import type { IconType } from "react-icons";
import {
  LuChartNoAxesCombined,
  LuClock3,
  LuDroplet,
  LuFileText,
  LuFolder,
  LuLayoutDashboard,
  LuLogOut,
  LuMenu,
  LuSquarePen,
  LuSquarePlus,
  LuX,
} from "react-icons/lu";

import { Logo } from "@/components/ui/Logo";

export type SideBarVariant = "monitoring" | "analysis-registration";

export interface SideBarItem {
  label: string;
  icon: IconType;
  href?: string;
  activePaths?: string[];
  onClick?: () => void;
}

export interface SideBarSection {
  title?: string;
  items: SideBarItem[];
}

export interface SideBarProps {
  variant?: SideBarVariant;
  title?: string;
  subtitle?: string;
  sections?: SideBarSection[];
  activeHref?: string;
  className?: string;
}

export const monitoringSideBarSections: SideBarSection[] = [
  {
    title: "Visualização",
    items: [
      {
        label: "Dashboard",
        href: "/dashboard",
        icon: LuLayoutDashboard,
      },
      {
        label: "Histórico",
        href: "/history",
        icon: LuClock3,
      },
      {
        label: "Análise",
        href: "/analysis",
        icon: LuChartNoAxesCombined,
      },
      {
        label: "Relatório",
        href: "/report",
        icon: LuFileText,
      },
    ],
  },
  {
    title: "Ações",
    items: [
      {
        label: "Inserir Dados",
        href: "/insert",
        icon: LuSquarePen,
      },
      {
        label: "Sair",
        href: "/water-bodies",
        icon: LuLogOut,
      },
    ],
  },
];

export const analysisRegistrationSideBarSections: SideBarSection[] = [
  {
    title: "Ações",
    items: [
      {
        label: "Adicionar corpo hídrico",
        href: "/add-water-bodie",
        icon: LuDroplet,
      },
      {
        label: "Nova análise",
        href: "/add-analysis",
        activePaths: ["/add-analysis", "/analysis"],
        icon: LuSquarePlus,
      },
      {
        label: "Minhas análises",
        href: "/my-analyses",
        icon: LuFolder,
      },
      {
        label: "Sair",
        href: "/water-bodies",
        icon: LuLogOut,
      },
    ],
  },
];

const sideBarDefaults: Record<
  SideBarVariant,
  {
    title: string;
    subtitle: string;
    sections: SideBarSection[];
  }
> = {
  monitoring: {
    title: "Water Path",
    subtitle: "Painel de Monitoramento",
    sections: monitoringSideBarSections,
  },
  "analysis-registration": {
    title: "Water Path",
    subtitle: "Cadastro de análises",
    sections: analysisRegistrationSideBarSections,
  },
};

export function SideBar({
  variant = "monitoring",
  title,
  subtitle,
  sections,
  activeHref,
  className = "",
}: SideBarProps) {
  const [isOpen, setIsOpen] = useState(false);
  const pathname = usePathname();
  const defaults = sideBarDefaults[variant];

  function closeSideBar() {
    setIsOpen(false);
  }

  return (
    <>
      <SideBarMobileHeader
        variant={variant}
        title={title ?? defaults.title}
        isOpen={isOpen}
        onOpen={() => setIsOpen(true)}
      />

      {isOpen && (
        <button
          type="button"
          aria-label="Fechar menu"
          onClick={closeSideBar}
          className="fixed inset-0 z-40 bg-black/35 lg:hidden"
        />
      )}

      <aside
        className={`${getAsideClassName(variant)} ${
          isOpen ? "translate-x-0" : "-translate-x-full"
        } ${className}`}
      >
        <button
          type="button"
          aria-label="Fechar menu"
          onClick={closeSideBar}
          className="absolute right-3 top-3 inline-flex h-9 w-9 items-center justify-center text-text-secondary transition-colors hover:text-primary lg:hidden"
        >
          <LuX className="h-6 w-6" />
        </button>

        <SideBarContent
          variant={variant}
          title={title ?? defaults.title}
          subtitle={subtitle ?? defaults.subtitle}
          sections={sections ?? defaults.sections}
          activeHref={activeHref ?? pathname}
          onNavigate={closeSideBar}
        />
      </aside>
    </>
  );
}

function SideBarMobileHeader({
  variant,
  title,
  isOpen,
  onOpen,
}: {
  variant: SideBarVariant;
  title: string;
  isOpen: boolean;
  onOpen: () => void;
}) {
  const isMonitoring = variant === "monitoring";

  return (
    <header
      className={`relative flex h-20 w-full items-center border-b border-placeholder px-6 shadow-[0_8px_15px_rgba(23,166,191,0.18)] lg:hidden ${
        isMonitoring ? "bg-background sm:px-8" : "bg-white"
      }`}
    >
      <button
        type="button"
        aria-label="Abrir menu"
        aria-expanded={isOpen}
        onClick={onOpen}
        className="inline-flex h-11 w-11 items-center justify-center text-primary transition-colors hover:text-secondary"
      >
        <LuMenu className="h-7 w-7" />
      </button>

      {isMonitoring ? (
        <Link
          href="/"
          className="absolute left-1/2 flex -translate-x-1/2 items-center gap-2"
        >
          <Logo className="h-9 w-auto shrink-0" />
          <h1 className="font-heading text-[24px] font-bold text-primary drop-shadow-[0_3px_3px_rgba(23,166,191,0.35)] sm:text-[28px]">
            {title}
          </h1>
        </Link>
      ) : (
        <SideBarBrand
          title={title}
          variant={variant}
          className="absolute left-1/2 -translate-x-1/2"
        />
      )}
    </header>
  );
}

function SideBarContent({
  variant,
  title,
  subtitle,
  sections,
  activeHref,
  onNavigate,
}: {
  variant: SideBarVariant;
  title: string;
  subtitle: string;
  sections: SideBarSection[];
  activeHref: string;
  onNavigate: () => void;
}) {
  if (variant === "analysis-registration") {
    return (
      <nav className="flex min-h-full flex-col px-5 pb-8 pt-8">
        <section className="flex flex-col gap-2">
          <SideBarBrand title={title} variant={variant} />

          <p className="font-heading text-[12px] font-medium text-text-secondary">
            {subtitle}
          </p>
        </section>

        <section className="mt-14 flex flex-col gap-10">
          {sections.map((section, index) => (
            <SideBarSectionList
              key={section.title ?? index}
              variant={variant}
              section={section}
              activeHref={activeHref}
              onNavigate={onNavigate}
            />
          ))}
        </section>
      </nav>
    );
  }

  return (
    <nav className="flex h-full flex-col gap-10">
      <section className="flex flex-col gap-2">
        <SideBarBrand title={title} variant={variant} />

        <p className="font-heading text-[11px] text-text-secondary">
          {subtitle}
        </p>
      </section>

      <section className="flex flex-col gap-11">
        {sections.map((section, index) => (
          <SideBarSectionList
            key={section.title ?? index}
            variant={variant}
            section={section}
            activeHref={activeHref}
            onNavigate={onNavigate}
          />
        ))}
      </section>
    </nav>
  );
}

function SideBarBrand({
  title,
  variant,
  className = "",
}: {
  title: string;
  variant: SideBarVariant;
  className?: string;
}) {
  const isMonitoring = variant === "monitoring";

  return (
    <Link href="/" className={`flex min-w-0 items-center gap-2 ${className}`}>
      <Logo className={`${isMonitoring ? "h-8" : "h-8"} w-auto shrink-0`} />
      <h1
        className={`truncate font-heading font-bold text-primary ${
          isMonitoring
            ? "text-[18px] drop-shadow-[0_3px_3px_rgba(23,166,191,0.22)]"
            : "text-[16px]"
        }`}
      >
        {title}
      </h1>
    </Link>
  );
}

function SideBarSectionList({
  variant,
  section,
  activeHref,
  onNavigate,
}: {
  variant: SideBarVariant;
  section: SideBarSection;
  activeHref: string;
  onNavigate: () => void;
}) {
  const isMonitoring = variant === "monitoring";

  return (
    <section className={`flex flex-col ${isMonitoring ? "gap-4" : "gap-5"}`}>
      {section.title && (
        <h2
          className={`font-primary text-[12px] text-text-secondary ${
            isMonitoring ? "font-medium" : "font-semibold"
          }`}
        >
          {section.title}
        </h2>
      )}

      <ul className={`flex flex-col ${isMonitoring ? "gap-5" : "gap-3"}`}>
        {section.items.map((item) => (
          <li
            key={item.href ?? `${section.title}-${item.label}`}
            className="relative"
          >
            <SideBarNavItem
              variant={variant}
              item={item}
              isActive={isSideBarItemActive(item, activeHref)}
              onNavigate={onNavigate}
            />
          </li>
        ))}
      </ul>
    </section>
  );
}

function SideBarNavItem({
  variant,
  item,
  isActive,
  onNavigate,
}: {
  variant: SideBarVariant;
  item: SideBarItem;
  isActive: boolean;
  onNavigate: () => void;
}) {
  const Icon = item.icon;
  const itemClassName =
    variant === "monitoring"
      ? `relative flex items-center gap-3 rounded-md py-1 pl-4 font-heading text-[13px] transition-colors hover:text-secondary ${
          isActive ? "text-secondary" : "text-text-primary"
        }`
      : `group ml-[-8px] flex min-h-11 w-[calc(100%+16px)] items-center gap-3 rounded-md px-2 pl-4 font-heading text-[12px] transition-colors ${
          isActive
            ? "bg-secondary/10 font-bold text-primary"
            : "font-medium text-text-primary hover:bg-primary/5 hover:text-primary"
        }`;

  function handleClick() {
    item.onClick?.();
    onNavigate();
  }

  const content =
    variant === "monitoring" ? (
      <>
        {isActive && (
          <span className="absolute left-0 h-9 w-1.5 rounded-full bg-secondary" />
        )}

        <Icon className="h-5 w-5 shrink-0" />
        <span>{item.label}</span>
      </>
    ) : (
      <>
        {isActive && (
          <span className="absolute left-[-14px] top-1/2 h-10 w-1.5 -translate-y-1/2 rounded-full bg-primary" />
        )}

        <Icon
          className={`h-5 w-5 shrink-0 ${
            isActive
              ? "text-primary"
              : "text-text-primary group-hover:text-primary"
          }`}
        />
        <span className="min-w-0 truncate">{item.label}</span>
      </>
    );

  if (item.href) {
    return (
      <Link
        href={item.href}
        onClick={handleClick}
        aria-current={isActive ? "page" : undefined}
        className={itemClassName}
      >
        {content}
      </Link>
    );
  }

  return (
    <button
      type="button"
      onClick={handleClick}
      className={`${itemClassName} text-left`}
    >
      {content}
    </button>
  );
}

function getAsideClassName(variant: SideBarVariant) {
  if (variant === "analysis-registration") {
    return `
      fixed inset-y-0 left-0 z-50 min-h-screen w-[240px] overflow-y-auto
      bg-white shadow-[8px_0_22px_rgba(18,33,45,0.08)] transition-transform duration-300
      lg:sticky lg:top-0 lg:z-auto lg:w-[214px] lg:translate-x-0 lg:rounded-r-md
    `;
  }

  return `
    fixed inset-y-0 left-0 z-50 min-h-screen w-[230px] border-r border-placeholder
    bg-background px-5 py-4 transition-transform duration-300 lg:static lg:z-auto
    lg:w-[210px] lg:translate-x-0
  `;
}

function isSideBarItemActive(item: SideBarItem, activeHref: string) {
  if (!item.href && !item.activePaths) {
    return false;
  }

  const paths = item.activePaths ?? (item.href ? [item.href] : []);

  return paths.some((path) => {
    if (path === "/") {
      return activeHref === "/";
    }

    return activeHref === path || activeHref.startsWith(`${path}/`);
  });
}
