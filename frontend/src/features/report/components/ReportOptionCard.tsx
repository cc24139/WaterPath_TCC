import type { ReactNode } from "react";
import { LuCircleAlert } from "react-icons/lu";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";

interface ReportOptionCardProps {
  number: number;
  title: string;
  children: ReactNode;
  className?: string;
}

export function ReportOptionCard({
  number,
  title,
  children,
  className = "",
}: ReportOptionCardProps) {
  return (
    <Card className={`w-full min-w-0 !px-4 !py-4 sm:!px-5 ${className}`}>
      <CardHeader className="mb-4 flex items-center gap-1.5">
        <CardTitle className="text-[12px] sm:text-[13px]">
          {number}. {title}
        </CardTitle>
        <LuCircleAlert className="h-3 w-3 text-placeholder" />
      </CardHeader>

      <CardContent>{children}</CardContent>
    </Card>
  );
}
