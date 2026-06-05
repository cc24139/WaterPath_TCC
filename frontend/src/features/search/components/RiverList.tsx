import Link from "next/link";
import Image from "next/image";
import { LuArrowRight } from "react-icons/lu";

import { Card } from "@/components/ui/Card";
import { mockRivers } from "@/features/search/constants/mockRivers";
import type { River } from "@/features/search/types/river";

interface RiverListProps {
  rivers?: River[];
  selectedRiverId?: string;
  className?: string;
}

export function RiverList({
  rivers = mockRivers,
  selectedRiverId,
  className = "",
}: RiverListProps) {
  return (
    <section className={`flex w-full flex-col gap-5 sm:gap-6 lg:gap-7 ${className}`}>
      {rivers.map((river) => (
        <RiverCard
          key={river.id}
          river={river}
          isSelected={river.id === selectedRiverId}
        />
      ))}
    </section>
  );
}

function RiverCard({
  river,
  isSelected,
}: {
  river: River;
  isSelected: boolean;
}) {
  return (
    <Card
      className={`w-full border-2 !rounded-lg !px-3 !py-3 transition-colors sm:!rounded-2xl sm:!px-5 sm:!py-5 ${
        isSelected ? "border-primary" : "border-transparent"
      }`}
    >
      <article className="grid gap-4 md:grid-cols-[minmax(160px,210px)_minmax(190px,1fr)] md:gap-5 xl:grid-cols-[minmax(170px,220px)_minmax(190px,1fr)_minmax(130px,170px)] xl:gap-6">
        <section className="min-w-0">
          <div className="flex min-w-0 items-baseline gap-1.5 sm:gap-2">
            <h2 className="truncate font-heading text-[17px] font-bold text-text-primary sm:text-[20px] lg:text-[22px]">
              {river.name}
            </h2>
            <span className="shrink-0 text-[9px] font-medium text-text-secondary sm:text-[10px] lg:text-[11px]">
              {river.location}
            </span>
          </div>

          <div className="relative mt-3 aspect-[16/9] w-full overflow-hidden rounded-lg bg-placeholder sm:mt-4 sm:rounded-2xl md:aspect-[4/3]">
            {river.imageUrl && (
              <Image
                src={river.imageUrl}
                alt={`Imagem do ${river.name}`}
                width={420}
                height={315}
                unoptimized
                sizes="(min-width: 1280px) 220px, (min-width: 768px) 35vw, 100vw"
                className="h-full w-full object-cover"
              />
            )}
          </div>
        </section>

        <section className="min-w-0 border-placeholder md:border-l md:px-5 xl:px-6">
          <div className="flex flex-wrap items-baseline gap-1">
            <h3 className="font-heading text-[12px] font-bold text-text-primary sm:text-[13px]">
              Evolução do IQA
            </h3>
            <span className="text-[9px] font-medium text-text-secondary sm:text-[10px]">
              - últimos 12 meses
            </span>
          </div>

          <SimpleLineChart values={river.chartData} />
        </section>

        <section className="flex min-w-0 flex-col border-placeholder md:col-span-2 xl:col-span-1 xl:border-l xl:pl-6">
          <h3 className="font-heading text-[13px] font-medium text-text-primary sm:text-[14px]">
            Informações:
          </h3>

          <dl className="mt-3 grid grid-cols-2 gap-x-3 gap-y-1 text-[10px] leading-tight text-text-secondary sm:mt-5 sm:text-[11px] xl:mt-6 xl:block xl:space-y-1">
            <InfoItem label="Ph" value={river.ph.toFixed(2)} />
            <InfoItem label="Turbidez" value={river.turbidity.toFixed(2)} />
            <InfoItem
              label="Oxigênio Dissolvido"
              value={river.dissolvedOxygen.toFixed(2)}
            />
            <InfoItem label="Temperatura" value={`${river.temperature}°C`} />
            <InfoItem label="IQA" value={river.iqa.toString()} isStrong />
          </dl>

          <div className="relative mt-3 h-6 overflow-hidden rounded-full bg-placeholder sm:mt-4 sm:h-7">
            <div
              className={`h-full rounded-full ${getIqaColorClassName(river.iqa)}`}
              style={{ width: `${river.iqa}%` }}
            />
            <span className="absolute inset-0 flex items-center justify-center font-heading text-[12px] font-bold text-white sm:text-[14px]">
              {river.iqa}%
            </span>
          </div>

          <Link
            href={`/water-bodies/${river.id}`}
            className="mt-4 inline-flex items-center gap-1 self-start font-heading text-[10px] font-medium text-primary transition-colors hover:text-secondary sm:mt-6 sm:text-[11px] xl:mt-7"
          >
            Ver Análise
            <LuArrowRight className="h-3 w-3" />
          </Link>
        </section>
      </article>
    </Card>
  );
}

function InfoItem({
  label,
  value,
  isStrong = false,
}: {
  label: string;
  value: string;
  isStrong?: boolean;
}) {
  return (
    <div className="flex gap-1">
      <dt>{label}:</dt>
      <dd className={isStrong ? "font-bold text-text-primary" : ""}>{value}</dd>
    </div>
  );
}

function SimpleLineChart({ values }: { values: number[] }) {
  const points = values
    .map((value, index) => `${getX(index, values.length)},${getY(value)}`)
    .join(" ");

  return (
    <div className="mt-3 sm:mt-6 lg:mt-8 xl:mt-10">
      <svg
        viewBox="0 0 240 150"
        role="img"
        aria-label="Gráfico de evolução do IQA"
        className="h-[105px] w-full sm:h-[120px] lg:h-[132px] xl:h-[140px]"
      >
        <line
          x1="24"
          y1="112"
          x2="220"
          y2="112"
          stroke="var(--color-text-secondary)"
          strokeWidth="1"
          opacity="0.55"
        />
        <line
          x1="24"
          y1="28"
          x2="24"
          y2="112"
          stroke="var(--color-text-secondary)"
          strokeWidth="1"
          opacity="0.55"
        />

        {[0, 30, 60, 90].map((label) => (
          <text
            key={label}
            x="2"
            y={getY(label) + 4}
            fontSize="10"
            fill="var(--color-text-secondary)"
          >
            {label}
          </text>
        ))}

        <polyline
          points={points}
          fill="none"
          stroke="var(--color-primary)"
          strokeWidth="3"
          strokeLinecap="round"
          strokeLinejoin="round"
        />

        {values.map((value, index) => (
          <circle
            key={`${value}-${index}`}
            cx={getX(index, values.length)}
            cy={getY(value)}
            r="4"
            fill="var(--color-background)"
            stroke="var(--color-primary)"
            strokeWidth="2"
          />
        ))}

        {["Jan", "Mar", "Mai", "Jul", "Set", "Nov"].map((month, index) => (
          <text
            key={month}
            x={24 + index * 36}
            y="142"
            fontSize="10"
            fill="var(--color-text-secondary)"
          >
            {month}
          </text>
        ))}
      </svg>
    </div>
  );
}

function getIqaColorClassName(iqa: number) {
  if (iqa >= 90) {
    return "bg-primary";
  }

  if (iqa >= 75) {
    return "bg-secondary";
  }

  if (iqa >= 50) {
    return "bg-[#FFB36B]";
  }

  return "bg-contrast";
}

function getX(index: number, total: number) {
  if (total <= 1) {
    return 24;
  }

  return 24 + (index / (total - 1)) * 184;
}

function getY(value: number) {
  const clampedValue = Math.min(100, Math.max(0, value));

  return 112 - (clampedValue / 100) * 84;
}
