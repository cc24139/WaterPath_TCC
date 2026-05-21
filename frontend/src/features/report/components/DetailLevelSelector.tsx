"use client";

import { useState } from "react";

import { detailLevelOptions } from "../constants/reportOptions";
import type { DetailLevel } from "../types/reportConfig";

interface DetailLevelSelectorProps {
  value: DetailLevel;
  onChange: (value: DetailLevel) => void;
}

export function DetailLevelSelector({
  value,
  onChange,
}: DetailLevelSelectorProps) {
  const selectedIndex = detailLevelOptions.findIndex(
    (option) => option.value === value,
  );
  const [draftPercent, setDraftPercent] = useState<number | null>(null);
  const currentPercent = selectedIndex * 50;
  const displayPercent = draftPercent ?? currentPercent;

  function snapToNearestLevel(percent = displayPercent) {
    const nextIndex = Math.round(percent / 50);
    const nextLevel = detailLevelOptions[nextIndex]?.value;

    if (nextLevel) {
      onChange(nextLevel);
      setDraftPercent(null);
    }
  }

  return (
    <div className="px-1 pt-1">
      <div className="grid grid-cols-3 font-heading text-[11px] font-bold sm:text-[12px]">
        {detailLevelOptions.map((option) => (
          <span
            key={option.value}
            className={`
              pb-3 text-center transition-colors
              ${value === option.value ? "text-text-primary" : "text-[#767676]"}
            `}
          >
            {option.label}
          </span>
        ))}
      </div>

      <div className="relative mx-[16.666%] h-6">
        <div className="absolute left-0 right-0 top-1/2 h-1 -translate-y-1/2 rounded-full bg-placeholder" />
        <div
          className="absolute left-0 top-1/2 h-1 -translate-y-1/2 rounded-full bg-primary"
          style={{ width: `${displayPercent}%` }}
        />
        <span
          className="pointer-events-none absolute top-1/2 h-5 w-5 -translate-x-1/2 -translate-y-1/2 rounded-full bg-primary shadow-[0_2px_7px_rgba(23,166,191,0.45)]"
          style={{ left: `${displayPercent}%` }}
        />

        <input
          type="range"
          min={0}
          max={100}
          step={1}
          value={displayPercent}
          aria-label="Nível de detalhamento"
          aria-valuetext={detailLevelOptions[selectedIndex]?.label}
          onChange={(event) => setDraftPercent(Number(event.target.value))}
          onPointerUp={(event) =>
            snapToNearestLevel(Number(event.currentTarget.value))
          }
          onMouseUp={(event) =>
            snapToNearestLevel(Number(event.currentTarget.value))
          }
          onTouchEnd={(event) =>
            snapToNearestLevel(Number(event.currentTarget.value))
          }
          onBlur={() => snapToNearestLevel()}
          className="absolute inset-x-0 top-1/2 h-9 w-full -translate-y-1/2 cursor-pointer opacity-0"
        />
      </div>
    </div>
  );
}
