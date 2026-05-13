import type { ReactNode } from "react";

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
    <div
      className="
        bg-white
        w-full
        min-h-[115px]
        sm:min-h-[125px]
        xl:min-h-[110px]
        rounded-xl
        sm:rounded-2xl
        shadow-default
        px-3 py-3
        sm:px-4 sm:py-4
        xl:px-6 xl:py-5
        flex flex-col items-center justify-center text-center gap-2
        xl:flex-row xl:text-left xl:justify-start xl:gap-5
      "
    >
      <div
        className={`
          w-10 h-10
          sm:w-12 sm:h-12
          xl:w-14 xl:h-14
          rounded-full
          bg-[#F7F7F7]
          flex items-center justify-center
          shrink-0
          ${colorClass}
        `}
      >
        {icon}
      </div>

      <div className="flex flex-col items-center xl:items-start">
        <h3
          className="
            font-heading font-bold
            text-text-primary
            text-[11px]
            sm:text-[12px]
            xl:text-[14px]
          "
        >
          {title}
        </h3>

        <strong
          className={`
            font-heading font-bold
            text-[16px]
            sm:text-[18px]
            xl:text-[20px]
            ${colorClass}
          `}
        >
          {value}
        </strong>

        <span
          className="
            font-heading font-medium
            text-[#767676]
            text-[9px]
            sm:text-[10px]
            xl:text-[12px]
            leading-tight
          "
        >
          {description}
        </span>
      </div>
    </div>
  );
}
