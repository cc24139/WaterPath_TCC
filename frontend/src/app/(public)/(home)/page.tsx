import Image from "next/image";
import Link from "next/link";
import type { IconType } from "react-icons";
import {
  LuArrowRight,
  LuBrainCircuit,
  LuChartNoAxesCombined,
  LuDatabase,
  LuDroplet,
  LuMonitorCheck,
} from "react-icons/lu";

import { Footer } from "@/components/layout/Footer";
import { Header } from "@/components/layout/Header";
import { Card } from "@/components/ui/Card";

const offers = [
  {
    title: "Histórico de medições",
    description:
      "Consulte as coletas registradas e acompanhe a evolução dos indicadores disponíveis.",
    icon: LuChartNoAxesCombined,
  },
  {
    title: "Análise por coleta com IA",
    description:
      "Envie medições e uma imagem para obter a análise integrada de uma coleta.",
    icon: LuBrainCircuit,
  },
  {
    title: "Monitoramento por corpo hídrico",
    description:
      "Veja os indicadores, o histórico e as análises registradas em cada corpo hídrico.",
    icon: LuMonitorCheck,
  },
];

const processSteps = [
  {
    title: "Registro da coleta",
    description: "As medições são cadastradas para o corpo hídrico selecionado.",
    icon: LuDroplet,
  },
  {
    title: "Organização",
    description: "Os dados ficam reunidos no histórico de medições.",
    icon: LuDatabase,
  },
  {
    title: "Análise com IA",
    description: "Uma imagem permite solicitar a análise integrada da coleta.",
    icon: LuBrainCircuit,
  },
  {
    title: "Acompanhamento",
    description: "Consulte os resultados e compare os registros ao longo do tempo.",
    icon: LuMonitorCheck,
  },
];

export default function Home() {
  return (
    <div className="min-h-screen bg-background text-text-primary">
      <Header />

      <main>
        <section className="mx-auto grid w-full max-w-7xl items-center gap-10 px-6 py-14 sm:px-10 lg:grid-cols-[0.8fr_1.2fr] lg:gap-16 lg:px-16 lg:py-20">
          <div className="flex flex-col items-start">
            <h1 className="font-heading text-[40px] font-extrabold leading-tight text-text-primary sm:text-[48px] lg:text-[56px]">
              Como estão nossos rios?
            </h1>

            <p className="mt-5 max-w-md text-[16px] leading-7 text-text-secondary sm:text-[18px]">
              Explore as medições registradas, acompanhe os rios e consulte as
              análises disponíveis.
            </p>

            <Link
              href="/add-analysis"
              className="mt-9 inline-flex min-h-12 items-center justify-center rounded-lg bg-contrast px-7 font-heading text-[16px] font-bold text-white shadow-[0_10px_18px_rgba(255,138,91,0.28)] transition-transform hover:-translate-y-0.5 hover:shadow-[0_12px_22px_rgba(255,138,91,0.35)]"
            >
              Começar análise
            </Link>
          </div>

          <div className="relative aspect-[16/9] w-full overflow-hidden rounded-2xl shadow-default">
            <Image
              src="/images/water-path-hero-river.png"
              alt="Rio atravessando uma cidade com áreas verdes nas margens"
              fill
              priority
              sizes="(min-width: 1024px) 56vw, 100vw"
              className="object-cover"
            />
          </div>
        </section>

        <section className="mx-auto w-full max-w-7xl px-6 pb-14 pt-6 sm:px-10 lg:px-16 lg:pb-20">
          <SectionHeading
            title="O que o Water Path oferece"
            subtitle="Ferramentas para registrar e consultar informações sobre a qualidade da água."
          />

          <div className="mt-9 grid gap-6 md:grid-cols-3">
            {offers.map((offer) => (
              <OfferCard key={offer.title} {...offer} />
            ))}
          </div>
        </section>

        <section className="bg-secondary/10 px-6 py-14 sm:px-10 lg:py-16">
          <div className="mx-auto w-full max-w-7xl">
            <SectionHeading
              title="Como funciona"
              subtitle="Do registro da coleta à consulta das medições e análises."
            />

            <div className="mt-10 grid gap-6 md:grid-cols-4 md:gap-4">
              {processSteps.map((step, index) => (
                <ProcessStep
                  key={step.title}
                  stepNumber={index + 1}
                  isLast={index === processSteps.length - 1}
                  {...step}
                />
              ))}
            </div>
          </div>
        </section>

        <section className="mx-auto w-full max-w-7xl px-6 py-14 sm:px-10 lg:px-16 lg:py-16">
          <div className="relative overflow-hidden rounded-2xl bg-[linear-gradient(135deg,var(--color-primary),var(--color-secondary))] px-7 py-8 text-white shadow-default sm:px-10 lg:flex lg:items-center lg:justify-between lg:gap-10">
            <div className="pointer-events-none absolute inset-x-1/3 bottom-0 h-28 rounded-[50%] border border-white/20" />
            <div className="relative max-w-xl">
              <h2 className="font-heading text-[24px] font-extrabold leading-tight">
                Explore os corpos hídricos
              </h2>
              <p className="mt-3 text-[15px] leading-7 text-white/95">
                Acesse as medições, os gráficos e as análises disponíveis para
                cada corpo hídrico cadastrado.
              </p>
            </div>

            <Link
              href="/water-bodies"
              className="relative mt-7 inline-flex min-h-12 items-center justify-center gap-2 rounded-lg bg-white px-7 font-heading text-[16px] font-bold text-primary shadow-[0_10px_18px_rgba(0,0,0,0.12)] transition-transform hover:-translate-y-0.5 lg:mt-0"
            >
              Ver corpos hídricos
              <LuArrowRight className="h-5 w-5" />
            </Link>
          </div>
        </section>
      </main>

      <Footer />
    </div>
  );
}

function SectionHeading({
  title,
  subtitle,
}: {
  title: string;
  subtitle: string;
}) {
  return (
    <div className="mx-auto max-w-3xl text-center">
      <h2 className="font-heading text-[28px] font-extrabold leading-tight text-text-primary sm:text-[32px]">
        {title}
      </h2>
      <p className="mt-3 text-[15px] leading-6 text-text-secondary sm:text-[16px]">
        {subtitle}
      </p>
    </div>
  );
}

function OfferCard({
  title,
  description,
  icon: Icon,
}: {
  title: string;
  description: string;
  icon: IconType;
}) {
  return (
    <Card className="!rounded-lg !px-6 !py-7">
      <div className="flex h-16 w-16 items-center justify-center rounded-full bg-secondary/15 text-primary">
        <Icon className="h-8 w-8" />
      </div>

      <h3 className="mt-6 font-heading text-[22px] font-extrabold leading-tight text-text-primary">
        {title}
      </h3>
      <p className="mt-4 text-[15px] leading-7 text-text-secondary">
        {description}
      </p>
    </Card>
  );
}

function ProcessStep({
  stepNumber,
  title,
  description,
  icon: Icon,
  isLast,
}: {
  stepNumber: number;
  title: string;
  description: string;
  icon: IconType;
  isLast: boolean;
}) {
  return (
    <div className="relative flex flex-col items-center text-center">
      {!isLast && (
        <LuArrowRight className="absolute left-[calc(50%+56px)] top-9 hidden h-8 w-8 text-primary/35 md:block" />
      )}

      <div className="flex h-20 w-20 items-center justify-center rounded-full border border-placeholder bg-white text-primary shadow-default">
        <Icon className="h-9 w-9" />
      </div>

      <h3 className="mt-5 font-heading text-[16px] font-extrabold text-text-primary">
        {stepNumber}. {title}
      </h3>
      <p className="mt-2 max-w-[220px] text-[14px] leading-6 text-text-secondary">
        {description}
      </p>
    </div>
  );
}
