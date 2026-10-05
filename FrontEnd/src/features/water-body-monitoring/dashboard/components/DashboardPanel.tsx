import type { ReactNode } from "react";
import Link from "next/link";
import { LuArrowRight } from "react-icons/lu";
import { Card } from "@/components/ui/Card";

export function DashboardPanel({ title, action, children, className = "" }: {
  title: string; action?: { label: string; href: string }; children: ReactNode; className?: string;
}) {
  return (
    <Card className={`min-w-0 !p-5 ${className}`}>
      <h2 className="mb-4 font-heading text-sm font-bold text-text-primary">{title}</h2>
      {children}
      {action && <Link href={action.href} className="mt-4 inline-flex items-center gap-1 text-xs font-medium text-primary underline-offset-4 hover:underline focus-visible:outline-primary">{action.label}<LuArrowRight aria-hidden="true" /></Link>}
    </Card>
  );
}
