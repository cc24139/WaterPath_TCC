import Image from "next/image";
import Link from "next/link";
import type { IconType } from "react-icons";
import {
  LuArrowDown,
  LuArrowRight,
  LuArrowUpRight,
  LuBookOpen,
  LuBrainCircuit,
  LuChevronDown,
  LuClipboardCheck,
  LuEye,
  LuFlaskConical,
  LuInfo,
} from "react-icons/lu";

import { Footer } from "@/components/layout/Footer";
import { Header } from "@/components/layout/Header";
import { Card } from "@/components/ui/Card";
import { metalOptions } from "@/features/add-analysis/constants/addAnalysisOptions";
import { RiskBadge } from "@/features/analysis-risk/components/RiskBadge";
import {
  methodologyIndicators,
  methodologyQuestions,
  methodologySteps,
} from "../constants/methodologyContent";

const sectionLinks = [
  { href: "#processo", label: "O processo" },
  { href: "#parametros", label: "Parâmetros" },
  { href: "#resultados", label: "Leitura dos resultados" },
  { href: "#duvidas", label: "Dúvidas frequentes" },
];

const resultTypes = [
  {
    title: "Medição informada",
    label: "Dados da coleta",
    icon: LuFlaskConical,
    description: "É o valor obtido em campo ou laboratório e cadastrado por você, com a unidade correspondente.",
  },
  {
    title: "Estimativa da IA",
    label: "Resultado de modelo",
    icon: LuBrainCircuit,
    description: "É uma concentração estimada a partir dos parâmetros da água. Aparece identificada e não substitui uma medição.",
  },
  {
    title: "Evidência visual",
    label: "Análise da imagem",
    icon: LuEye,
    description: "São os sinais identificados na fotografia. As marcações ajudam a entender o que foi detectado naquela imagem.",
  },
];

export function MethodologyPage() {
  return (
    <div className="min-h-screen bg-background text-text-primary">
      <Header />

      <main>
        <section className="mx-auto grid w-full max-w-7xl items-center gap-10 px-6 pb-10 pt-12 sm:px-10 lg:grid-cols-[1.05fr_1fr] lg:gap-16 lg:px-16 lg:pb-12 lg:pt-16" aria-labelledby="methodology-title">
          <div>
            <Eyebrow>Conheça a metodologia</Eyebrow>
            <h1 id="methodology-title" className="mt-4 max-w-xl font-heading text-[36px] font-extrabold leading-[1.15] sm:text-[44px] lg:text-[50px]">
              Como são feitas<br className="hidden sm:block" /> as nossas <span className="text-primary">análises</span>
            </h1>
            <p className="mt-5 max-w-lg text-[15px] leading-7 text-text-secondary sm:text-[16px]">
              Cada coleta conta uma parte da história de um rio. Entenda como o WaterPath reúne medições e inteligência artificial para ajudar a acompanhar a qualidade da água.
            </p>
            <a href="#processo" className="mt-7 inline-flex min-h-12 items-center gap-3 rounded-lg bg-contrast px-6 font-heading text-sm font-bold text-white shadow-default transition-colors hover:bg-primary/90 focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-primary">
              Entenda o processo <LuArrowDown aria-hidden="true" className="h-4 w-4" />
            </a>
          </div>

          <div className="relative">
            <div className="relative aspect-[4/3] overflow-hidden rounded-2xl shadow-default sm:aspect-[16/10] lg:aspect-[4/3]">
              <Image src="/images/water-path-hero-river.png" alt="Vista ilustrativa de um rio urbano com vegetação nas margens" fill priority sizes="(min-width: 1024px) 44vw, 100vw" className="object-cover" />
              <div className="absolute inset-0 bg-gradient-to-t from-black/55 via-transparent to-transparent" />
              <div className="absolute inset-x-5 bottom-5 flex items-center gap-3 text-white sm:inset-x-6 sm:bottom-6">
                <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl border border-white/30 bg-white/15 backdrop-blur-sm"><LuClipboardCheck aria-hidden="true" className="h-6 w-6" /></span>
                <div><p className="font-heading text-sm font-bold">Da coleta ao acompanhamento</p><p className="mt-1 text-xs leading-5 text-white/90">Mais contexto para interpretar cada resultado.</p></div>
              </div>
            </div>
          </div>
        </section>

        <nav aria-label="Nesta página" className="mx-auto w-full max-w-7xl px-6 pb-12 sm:px-10 lg:px-16">
          <ul className="flex flex-wrap gap-2 border-b border-placeholder pb-6">
            {sectionLinks.map((link) => <li key={link.href}><a href={link.href} className="inline-flex min-h-10 items-center rounded-full border border-placeholder bg-white px-4 text-xs font-semibold text-text-secondary transition-colors hover:border-primary hover:text-primary focus-visible:outline-primary">{link.label}</a></li>)}
          </ul>
        </nav>

        <section id="processo" className="mx-auto w-full max-w-7xl scroll-mt-8 px-6 pb-14 sm:px-10 lg:px-16 lg:pb-20" aria-labelledby="process-title">
          <SectionIntro id="process-title" eyebrow="01 · O processo" title="Um caminho, diferentes evidências" description="As medições e a imagem se complementam. Cada resultado mantém sua origem para que você saiba o que foi registrado e o que foi estimado." />
          <ol className="mt-9 grid gap-4 sm:grid-cols-2 lg:grid-cols-5">
            {methodologySteps.map((step, index) => {
              const Icon = step.icon;
              return <li key={step.title} className="relative flex gap-4 rounded-xl border border-placeholder/70 bg-white p-5 sm:block">
                <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary"><Icon aria-hidden="true" className="h-6 w-6" /></div>
                <div>
                  <span className="font-heading text-[11px] font-bold tracking-widest text-primary sm:mt-5 sm:block">0{index + 1}</span>
                  <h3 className="mt-1 font-heading text-sm font-extrabold">{step.title}</h3>
                  <p className="mt-2 text-[13px] leading-6 text-text-secondary">{step.description}</p>
                </div>
                {index < methodologySteps.length - 1 && <LuArrowRight aria-hidden="true" className="absolute -right-3 top-9 z-10 hidden h-6 w-6 rounded-full border border-placeholder bg-background p-1 text-primary lg:block" />}
              </li>;
            })}
          </ol>
          <p className="mt-5 flex items-start gap-2 text-[13px] leading-6 text-text-secondary"><LuInfo aria-hidden="true" className="mt-1 h-4 w-4 shrink-0 text-primary" />Você também pode apenas salvar uma coleta. A IA só é executada quando a análise é solicitada.</p>
        </section>

        <section id="parametros" className="scroll-mt-8 border-y border-primary/10 bg-primary/5 px-6 py-14 sm:px-10 lg:px-16 lg:py-20" aria-labelledby="parameters-title">
          <div className="mx-auto grid w-full max-w-[1152px] items-start gap-10 lg:grid-cols-[0.75fr_1.25fr] lg:gap-16">
            <div className="lg:sticky lg:top-8">
              <Eyebrow>02 · Parâmetros da água</Eyebrow>
              <h2 id="parameters-title" className="mt-3 font-heading text-[32px] font-extrabold leading-tight text-primary sm:text-[38px]">O que cada<br className="hidden lg:block" /> medição revela</h2>
              <p className="mt-4 max-w-md text-sm leading-7 text-text-secondary">Um indicador isolado não conta a história inteira. Conheça os parâmetros do cadastro e o papel de cada um na análise.</p>
              <div className="mt-6 flex gap-7 border-t border-primary/20 pt-5">
                <div><p className="font-heading text-3xl font-extrabold text-primary">{methodologyIndicators.length.toString().padStart(2, "0")}</p><p className="mt-1 text-xs text-text-secondary">indicadores da água</p></div>
                <div><p className="font-heading text-3xl font-extrabold text-primary">{methodologyIndicators.filter(field => field.required).length.toString().padStart(2, "0")}</p><p className="mt-1 text-xs text-text-secondary">obrigatórios na coleta</p></div>
              </div>
              <p className="mt-5 text-[13px] leading-6 text-text-secondary">Abra cada parâmetro para saber mais. As unidades são as mesmas utilizadas no formulário.</p>
            </div>

            <div className="relative space-y-3 pl-7 before:absolute before:bottom-7 before:left-[5px] before:top-7 before:w-px before:bg-primary/30 sm:pl-8">
              {methodologyIndicators.map((field, index) => {
                const Icon = field.icon;
                return <details key={field.name} name="water-parameters" open={index === 0} className="group relative rounded-xl border border-placeholder/70 bg-white shadow-[0_3px_12px_rgba(0,0,0,0.025)] open:border-primary/40">
                  <span aria-hidden="true" className="absolute -left-[27px] top-7 h-3 w-3 rotate-45 border-2 border-background bg-primary sm:-left-8" />
                  <summary className="flex min-h-20 cursor-pointer list-none items-center gap-3 rounded-xl px-4 py-4 focus-visible:outline-primary [&::-webkit-details-marker]:hidden sm:px-5">
                    <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/8 text-primary"><Icon aria-hidden="true" className="h-5 w-5" /></span>
                    <span className="min-w-0 flex-1"><span className="block font-heading text-sm font-bold">{field.label}</span><span className="mt-1 block text-[11px] leading-5 text-text-secondary">{field.symbol}{field.unit !== field.symbol && ` · ${field.unit}`}{field.required ? " · Obrigatório" : " · Opcional"}</span></span>
                    <LuChevronDown aria-hidden="true" className="h-4 w-4 shrink-0 text-text-secondary transition-transform group-open:rotate-180" />
                  </summary>
                  <div className="border-t border-placeholder/50 px-4 pb-5 pt-4 sm:px-5">
                    <p className="font-heading text-xs font-bold text-primary">{field.summary}</p>
                    <p className="mt-2 text-[13px] leading-6 text-text-secondary">{field.description}</p>
                    <p className="mt-3 rounded-lg bg-background/70 px-3 py-2 text-[13px] leading-6"><span className="font-semibold">No WaterPath: </span>{field.usage}</p>
                  </div>
                </details>;
              })}
            </div>
          </div>

          <Card className="mx-auto mt-10 w-full max-w-[1152px] !p-5 sm:!p-7">
            <div className="grid gap-6 lg:grid-cols-[0.75fr_1.25fr] lg:gap-16">
              <div>
                <div className="flex items-center gap-2 text-primary"><LuFlaskConical aria-hidden="true" className="h-5 w-5" /><h3 className="font-heading text-lg font-extrabold text-text-primary">E os metais pesados?</h3></div>
                <p className="mt-3 text-[13px] leading-6 text-text-secondary">Cadastre apenas os metais que foram medidos. Os valores estimados pela IA são apresentados separadamente, com a origem identificada.</p>
              </div>
              <ul className="grid grid-cols-2 gap-2 sm:grid-cols-4" aria-label="Metais disponíveis e unidades">
                {metalOptions.map((metal) => <li key={metal.symbol} className="rounded-lg border border-placeholder/70 bg-background/40 px-3 py-3"><span className="font-heading text-lg font-extrabold text-primary">{metal.symbol}</span><span className="mt-1 block text-xs font-semibold">{metal.label}</span><span className="mt-1 block text-[11px] text-text-secondary">{metal.unit}</span></li>)}
              </ul>
            </div>
          </Card>
        </section>

        <section id="resultados" className="mx-auto w-full max-w-7xl scroll-mt-8 px-6 py-14 sm:px-10 lg:px-16 lg:py-20" aria-labelledby="results-title">
          <SectionIntro id="results-title" eyebrow="03 · Leitura dos resultados" title="Saber de onde vem faz diferença" description="Os resultados não têm todos a mesma origem. A distinção acompanha a coleta, os detalhes da análise e o relatório." />
          <div className="mt-8 grid gap-4 md:grid-cols-3">
            {resultTypes.map((type) => <ResultTypeCard key={type.title} {...type} />)}
          </div>
          <div className="mt-6 grid gap-6 rounded-xl border border-primary/20 bg-primary/5 p-5 sm:p-7 lg:grid-cols-[1.2fr_1fr] lg:gap-10">
            <div><h3 className="font-heading text-lg font-extrabold">Como funciona o nível de risco?</h3><p className="mt-3 text-[13px] leading-6 text-text-secondary">A triagem considera pH, oxigênio dissolvido, sinais visuais e a recorrência de alertas nas coletas anteriores. O resultado vem acompanhado dos motivos avaliados.</p><p className="mt-2 text-[13px] leading-6 text-text-secondary">Os metais são apresentados com sua origem e não compõem diretamente essa classificação. Risco e IQA são informações diferentes.</p></div>
            <div className="flex flex-col justify-center gap-3">
              <div className="flex flex-wrap gap-2" aria-label="Os três níveis de risco"><RiskBadge level={1} /><RiskBadge level={2} /><RiskBadge level={3} /></div>
              <p className="text-[13px] leading-6 text-text-secondary">Sem análise associada, a coleta permanece sem classificação de risco. Ausência de resultado não é convertida em risco baixo.</p>
            </div>
          </div>
        </section>

        <section id="duvidas" className="mx-auto w-full max-w-7xl scroll-mt-8 px-6 pb-14 sm:px-10 lg:px-16 lg:pb-20" aria-labelledby="questions-title">
          <div className="grid items-start gap-8 lg:grid-cols-[0.75fr_1.25fr] lg:gap-16">
            <SectionIntro id="questions-title" eyebrow="04 · Dúvidas frequentes" title="Para interpretar com mais clareza" description="Algumas respostas para usar as informações com o contexto que elas precisam." />
            <div className="space-y-3">
              {methodologyQuestions.map((item) => <details key={item.question} name="methodology-questions" className="group rounded-xl border border-placeholder bg-white">
                <summary className="flex min-h-16 cursor-pointer list-none items-center justify-between gap-4 rounded-xl px-5 py-4 font-heading text-sm font-bold focus-visible:outline-primary [&::-webkit-details-marker]:hidden">{item.question}<LuChevronDown aria-hidden="true" className="h-4 w-4 shrink-0 text-primary transition-transform group-open:rotate-180" /></summary>
                <div className="px-5 pb-5 text-[13px] leading-6 text-text-secondary"><p>{item.answer}</p>{item.reference && <a href={item.reference.href} target="_blank" rel="noopener noreferrer" className="mt-3 inline-flex items-center gap-1 font-semibold text-primary underline underline-offset-4">{item.reference.label}<LuArrowUpRight aria-hidden="true" className="h-4 w-4" /><span className="sr-only">(abre em nova aba)</span></a>}</div>
              </details>)}
            </div>
          </div>

          <div className="mt-10 flex flex-col gap-4 border-t border-placeholder pt-6 sm:flex-row sm:items-start sm:justify-between">
            <div className="flex items-start gap-3"><LuBookOpen aria-hidden="true" className="mt-0.5 h-5 w-5 shrink-0 text-primary" /><div><h3 className="font-heading text-sm font-bold">Para continuar aprendendo</h3><p className="mt-1 max-w-md text-[13px] leading-6 text-text-secondary">Referências sobre os indicadores de qualidade da água. A triagem do WaterPath tem os critérios descritos nesta página.</p></div></div>
            <div className="flex shrink-0 flex-col gap-3 text-xs font-semibold text-primary">
              <ReferenceLink href="https://www.ana.gov.br/portalpnqa/indicadores-indice-aguas.aspx">ANA · Indicadores e IQA</ReferenceLink>
              <ReferenceLink href="https://cetesb.sp.gov.br/wp-content/uploads/2022/04/variaveis.pdf">CETESB · Variáveis de qualidade da água (PDF)</ReferenceLink>
            </div>
          </div>
        </section>

        <section className="mx-auto w-full max-w-7xl px-6 pb-14 sm:px-10 lg:px-16 lg:pb-20" aria-labelledby="explore-title">
          <div className="flex flex-col items-start gap-6 rounded-2xl bg-[linear-gradient(120deg,var(--color-primary),#1595AC)] p-7 text-white sm:p-9 lg:flex-row lg:items-center lg:justify-between">
            <div><h2 id="explore-title" className="font-heading text-2xl font-extrabold">Veja a metodologia em prática</h2><p className="mt-3 max-w-xl text-sm leading-7 text-white/90">Explore os corpos hídricos e acompanhe as medições e análises disponíveis para cada rio.</p></div>
            <Link href="/water-bodies" className="inline-flex min-h-12 shrink-0 items-center gap-3 rounded-lg bg-white px-6 font-heading text-sm font-bold text-primary shadow-default transition-transform hover:-translate-y-0.5 focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-white">Explorar corpos hídricos <LuArrowRight aria-hidden="true" className="h-4 w-4" /></Link>
          </div>
        </section>
      </main>

      <Footer />
    </div>
  );
}

function Eyebrow({ children }: { children: React.ReactNode }) {
  return <p className="font-heading text-[11px] font-extrabold uppercase tracking-[0.16em] text-primary">{children}</p>;
}

function SectionIntro({ id, eyebrow, title, description }: { id: string; eyebrow: string; title: string; description: string }) {
  return <div className="max-w-2xl"><Eyebrow>{eyebrow}</Eyebrow><h2 id={id} className="mt-3 font-heading text-[26px] font-extrabold leading-tight sm:text-[30px]">{title}</h2><p className="mt-3 text-sm leading-7 text-text-secondary">{description}</p></div>;
}

function ResultTypeCard({ title, label, description, icon: Icon }: { title: string; label: string; description: string; icon: IconType }) {
  return <Card className="!p-6"><div className="flex h-11 w-11 items-center justify-center rounded-xl bg-primary/10 text-primary"><Icon aria-hidden="true" className="h-5 w-5" /></div><p className="mt-5 text-[10px] font-bold uppercase tracking-wider text-primary">{label}</p><h3 className="mt-2 font-heading text-lg font-extrabold">{title}</h3><p className="mt-3 text-[13px] leading-6 text-text-secondary">{description}</p></Card>;
}

function ReferenceLink({ href, children }: { href: string; children: React.ReactNode }) {
  return <a href={href} target="_blank" rel="noopener noreferrer" className="inline-flex items-center gap-1.5 underline decoration-primary/30 underline-offset-4 hover:decoration-primary">{children}<LuArrowUpRight aria-hidden="true" className="h-4 w-4 shrink-0" /><span className="sr-only">(abre em nova aba)</span></a>;
}
