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
    <Card className={`flex h-full min-w-0 flex-col !px-5 !py-5 sm:!px-6 ${className}`}>
      <CardHeader className="mb-4 flex items-center gap-2">
        <CardTitle className="text-[16px] sm:text-[17px]">{title}</CardTitle>
        <LuCircleAlert className="h-3.5 w-3.5 text-placeholder" />
      </CardHeader>

      <CardContent className="flex flex-1 flex-col gap-4">
        {items.map((item) => (
          <RiverInsightListItem key={item.title} item={item} />
        ))}
      </CardContent>

      <CardFooter className="!mt-auto flex justify-end pt-5">
        <Link href={actionHref} className="inline-flex items-center gap-1.5 font-heading text-xs font-semibold text-primary transition-colors hover:text-secondary focus-visible:outline-2 focus-visible:outline-primary">
          {actionLabel}<LuArrowRight className="h-4 w-4" aria-hidden="true" />
        </Link>
      </CardFooter>
    </Card>
  );
}

function RiverInsightListItem({ item }: { item: RiverInsightItem }) {
  return (
    <article className="flex items-start gap-4">
      <div
        className={`
          flex h-10 w-10 shrink-0 items-center justify-center
          rounded-full bg-background
          ${item.colorClass}
        `}
      >
        {item.icon}
      </div>

      <div className="min-w-0 pt-0.5">
        <h3 className="font-heading text-xs font-bold leading-snug text-text-primary sm:text-sm">
          {item.title}
        </h3>
        <p className="mt-1 text-xs leading-relaxed text-text-secondary">
          {item.description}
        </p>
      </div>
    </article>
  );
}
