"use client";

import { useId, useState, type ReactNode } from "react";
import { LuCircleAlert } from "react-icons/lu";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";

interface ReportOptionCardProps {
  number: number;
  title: string;
  description?: string;
  children: ReactNode;
  className?: string;
}

export function ReportOptionCard({
  number,
  title,
  description,
  children,
  className = "",
}: ReportOptionCardProps) {
  const [showDescription, setShowDescription] = useState(false);
  const descriptionId = useId();

  return (
    <Card className={`w-full min-w-0 !px-4 !py-4 sm:!px-5 ${className}`}>
      <CardHeader className="mb-4">
        <div className="flex items-center gap-1.5">
          <CardTitle className="text-[12px] sm:text-[13px]">
            {number}. {title}
          </CardTitle>
          {description && <button
            type="button"
            aria-label={`Ajuda sobre ${title.toLocaleLowerCase("pt-BR")}`}
            aria-expanded={showDescription}
            aria-controls={descriptionId}
            onClick={() => setShowDescription(value => !value)}
            onKeyDown={event => { if (event.key === "Escape") setShowDescription(false); }}
            className="inline-flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-text-secondary transition-colors hover:bg-primary/10 hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40"
          >
            <LuCircleAlert aria-hidden="true" className="h-3.5 w-3.5" />
          </button>}
        </div>
        {description && <p id={descriptionId} hidden={!showDescription} className="mt-2 rounded-md bg-primary/5 px-3 py-2 text-xs leading-relaxed text-text-secondary">
          {description}
        </p>}
      </CardHeader>

      <CardContent>{children}</CardContent>
    </Card>
  );
}
