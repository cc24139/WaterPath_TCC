import type { ReactNode } from "react";
import type { IconType } from "react-icons";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";

export interface FormSectionCardProps {
  title: string;
  description: string;
  icon: IconType;
  children: ReactNode;
  headerAction?: ReactNode;
  className?: string;
}

export function FormSectionCard({
  title,
  description,
  icon: Icon,
  children,
  headerAction,
  className = "",
}: FormSectionCardProps) {
  return (
    <Card className={`!rounded-lg !px-4 !py-5 sm:!px-5 lg:!px-6 ${className}`}>
      <CardHeader className="mb-5 flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="flex min-w-0 items-start gap-4">
          <span className="inline-flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-primary text-white shadow-[0_8px_18px_rgba(23,166,191,0.22)]">
            <Icon className="h-5 w-5" />
          </span>
          <span className="min-w-0">
            <CardTitle className="text-[18px] leading-tight sm:text-[20px]">
              {title}
            </CardTitle>
            <p className="mt-1 font-heading text-[12px] font-medium leading-relaxed text-text-secondary sm:text-[13px]">
              {description}
            </p>
          </span>
        </div>

        {headerAction ? <div className="shrink-0">{headerAction}</div> : null}
      </CardHeader>

      <CardContent>{children}</CardContent>
    </Card>
  );
}
