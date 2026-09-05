"use client";

import { useState } from "react";
import { LuChevronRight } from "react-icons/lu";

import { Card } from "@/components/ui/Card";
import { getRiverStatusClassName } from "../utils/riverData";
import type { River, RiverStatus } from "@/features/search/types/river";

interface RiverListSelectProps {
  rivers: River[];
  selectedRiverId?: string;
  onSelectRiver?: (river: River) => void;
  monitoredCount?: number;
  className?: string;
}

export function RiverListSelect({
  rivers,
  selectedRiverId,
  onSelectRiver,
  monitoredCount = rivers.length,
  className = "",
}: RiverListSelectProps) {
  const isControlled = onSelectRiver !== undefined;
  const [internalSelectedRiverId, setInternalSelectedRiverId] = useState(
    selectedRiverId
  );
  const currentSelectedRiverId = isControlled
    ? selectedRiverId
    : internalSelectedRiverId;

  function handleSelectRiver(river: River) {
    if (!isControlled) {
      setInternalSelectedRiverId((currentRiverId) =>
        currentRiverId === river.id ? undefined : river.id
      );
    }

    onSelectRiver?.(river);
  }

  return (
    <Card
      className={`
        w-full max-w-[360px] min-h-0 lg:min-h-[520px]
        !rounded-lg !px-0 !py-4 sm:!py-5 lg:!py-6
        ${className}
      `}
    >
      <header className="px-4 sm:px-5">
        <h2 className="font-heading text-[16px] font-semibold leading-tight text-text-primary sm:text-[18px] lg:text-[20px]">
          Corpos hídricos:
        </h2>
        <p className="mt-1 text-[10px] font-medium text-text-secondary sm:text-[11px] lg:text-[12px]">
          {monitoredCount} encontrado(s)
        </p>
      </header>

      <ul className="mt-4 flex max-h-[220px] flex-col gap-1 overflow-y-auto sm:mt-6 sm:max-h-[280px] lg:mt-9 lg:max-h-[65vh]">
        {rivers.map((river) => {
          const isSelected = river.id === currentSelectedRiverId;

          return (
            <li key={river.id}>
              <button
                type="button"
                aria-pressed={isSelected}
                onClick={() => handleSelectRiver(river)}
                className={`
                  relative grid min-h-9 w-full grid-cols-[minmax(0,1fr)_auto]
                  items-center gap-2 py-1.5 pl-5 pr-4 text-left
                  transition-colors hover:bg-secondary/10
                  sm:min-h-10 sm:gap-3 sm:pl-6 sm:pr-5
                  ${isSelected ? "bg-secondary/5" : ""}
                `}
              >
                {isSelected && (
                  <span className="absolute left-0 h-7 w-1 bg-secondary sm:h-8 sm:w-1.5" />
                )}

                <span className="flex min-w-0 flex-col gap-1">
                  <span className="truncate font-heading text-[13px] font-bold text-text-primary sm:text-[15px] lg:text-[16px]">
                    {river.name}
                  </span>
                  <span className="w-full truncate text-[9px] font-medium text-text-secondary sm:text-[10px] lg:text-[11px]">
                    {river.location}
                  </span>
                </span>

                <RiverStatusBadge status={river.status} />
              </button>
            </li>
          );
        })}
      </ul>
    </Card>
  );
}

function RiverStatusBadge({ status }: { status: RiverStatus }) {
  return (
    <span
      className={`
        inline-flex min-w-[76px] items-center justify-center gap-1
        rounded-full px-2 py-1 font-heading text-[10px] font-bold
        sm:min-w-[76px] sm:px-3 sm:py-1.5 sm:text-[12px]
        ${getRiverStatusClassName(status)}
      `}
    >
      {status}
      <LuChevronRight className="h-3 w-3 shrink-0 sm:h-3.5 sm:w-3.5" />
    </span>
  );
}
