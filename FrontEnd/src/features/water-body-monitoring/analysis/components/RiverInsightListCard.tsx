import type { ReactNode } from "react";
import Link from "next/link";
import { LuArrowRight, LuCircleAlert } from "react-icons/lu";

import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/Card";

interface RiverInsightItem {
  title: string;
  description: string;
  icon: ReactNode;
  colorClass: string;
}

interface RiverInsightListCardProps {
  title: string;
  items: RiverInsightItem[];
  actionLabel: string;
  actionHref: string;
  className?: string;
}

export function RiverInsightListCard({
  title,
  items,
  actionLabel,
  actionHref,
  className = "",
}: RiverInsightListCardProps) {
  return (
    <Card className={`!px-5 !py-5 sm:!px-6 ${className}`}>
      <CardHeader className="mb-4 flex items-center gap-2">
        <CardTitle className="text-[16px] sm:text-[17px]">{title}</CardTitle>
        <LuCircleAlert className="h-3.5 w-3.5 text-placeholder" />
      </CardHeader>

      <CardContent className="flex flex-col gap-5">
        {items.map((item) => (
          <RiverInsightListItem key={item.title} item={item} />
        ))}
      </CardContent>

      <CardFooter className="mt-5 flex justify-end">
        {actionHref.startsWith("/") ? (
          <Link
            href={actionHref}
            className="inline-flex items-center gap-1 font-heading text-[13px] font-medium text-primary transition-colors hover:text-secondary"
          >
            {actionLabel}
            <LuArrowRight className="h-3.5 w-3.5" />
          </Link>
        ) : (
          <a
            href={actionHref}
            className="inline-flex items-center gap-1 font-heading text-[13px] font-medium text-primary transition-colors hover:text-secondary"
          >
            {actionLabel}
            <LuArrowRight className="h-3.5 w-3.5" />
          </a>
        )}
      </CardFooter>
    </Card>
  );
}

function RiverInsightListItem({ item }: { item: RiverInsightItem }) {
  return (
    <article className="flex items-start gap-4">
      <div
        className={`
          flex h-11 w-11 shrink-0 items-center justify-center
          rounded-full bg-[#F7F7F7] sm:h-12 sm:w-12
          ${item.colorClass}
        `}
      >
        {item.icon}
      </div>

      <div className="min-w-0 pt-0.5">
        <h3 className="font-heading text-[12px] font-bold leading-tight text-text-primary sm:text-[13px]">
          {item.title}
        </h3>
        <p className="mt-1 font-heading text-[9px] font-medium leading-snug text-[#767676] sm:text-[10px]">
          {item.description}
        </p>
      </div>
    </article>
  );
}
