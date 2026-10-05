"use client";

import { SideBar, monitoringSideBarSections } from "@/components/layout/SideBar";

const monitoringPaths = new Set(["/dashboard", "/history", "/analysis", "/report", "/insert"]);

export function MonitoringSidebar({ waterBodyId }: { waterBodyId: string }) {
  const basePath = `/water-bodies/${encodeURIComponent(waterBodyId)}`;
  const sections = monitoringSideBarSections.map((section) => ({
    ...section,
    items: section.items.map((item) => ({
      ...item,
      label: item.href === "/water-bodies" ? "Voltar aos corpos hídricos" : item.label,
      href: item.href && monitoringPaths.has(item.href) ? `${basePath}${item.href}` : item.href,
      // The listing is an ancestor of monitoring routes; never mark it active here.
      activePaths: item.href === "/water-bodies" ? [] : item.activePaths?.map((path) => monitoringPaths.has(path) ? `${basePath}${path}` : path),
    })),
  }));

  return <SideBar variant="monitoring" sections={sections} />;
}
