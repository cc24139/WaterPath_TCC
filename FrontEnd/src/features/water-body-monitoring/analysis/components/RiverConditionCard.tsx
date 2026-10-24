import type { ReactNode } from "react";

import { Card } from "@/components/ui/Card";

interface RiverConditionCardProps {
  icon: ReactNode;
  title: string;
  value: string;
  description: string;
  colorClass: string;
}

export function RiverConditionCard({
  icon,
  title,
  value,
  description,
  colorClass,
}: RiverConditionCardProps) {
  return (
    <Card className="flex min-h-[116px] w-full min-w-0 items-center gap-3 !p-4 sm:gap-4">
      <div className={`flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-background ${colorClass}`}>
        {icon}
      </div>
      <div className="min-w-0 text-left">
        <h3 className="font-heading text-xs font-semibold leading-tight text-text-primary">{title}</h3>
        <strong className={`mt-1 block break-words font-heading text-base font-bold leading-tight sm:text-lg ${colorClass}`}>{value}</strong>
        <p className="mt-1 text-[11px] leading-snug text-text-secondary sm:text-xs">{description}</p>
      </div>
    </Card>
  );
}
