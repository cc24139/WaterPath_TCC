import type { ReactNode } from "react";
import Link from "next/link";
import { LuArrowRight } from "react-icons/lu";
import { Card } from "@/components/ui/Card";

export function DashboardPanel({ title, action, children, className = "" }: {
  title: string; action?: { label: string; href: string }; children: ReactNode; className?: string;
}) {
  return (
    <Card className={`flex min-w-0 flex-col !p-5 ${className}`}>
      <h2 className="mb-4 font-heading text-sm font-bold text-text-primary">{title}</h2>
      <div className="min-w-0 flex-1">{children}</div>
      {action && <div className="mt-auto pt-4"><Link href={action.href} className="inline-flex w-fit items-center gap-1.5 rounded-md bg-primary/5 px-3 py-2 text-xs font-semibold text-primary transition-colors hover:bg-primary/10 focus-visible:outline-2 focus-visible:outline-primary">{action.label}<LuArrowRight aria-hidden="true" /></Link></div>}
    </Card>
  );
}
