import { kenia } from "../../app/fonts";
import { Logo } from "./Logo";

type LogoParabolaProps = {
  className?: string;
};

export function LogoParabola({ className = "" }: LogoParabolaProps) {
  return (
    <section
      aria-label="Water Path - Monitoramento inteligente da qualidade da água"
      className={`relative min-h-[320px] w-full max-w-[360px] shrink-0 overflow-hidden sm:min-h-[420px] sm:max-w-[420px] md:min-h-screen md:max-w-[440px] lg:max-w-[470px] xl:max-w-[500px] ${className}`}
    >
      <svg
        aria-hidden="true"
        className="absolute inset-0 h-full w-full"
        preserveAspectRatio="xMinYMid slice"
        viewBox="0 0 304 720"
        xmlns="http://www.w3.org/2000/svg"
      >
        <defs>
          <linearGradient id="logo-parabola-fill" x1="136" x2="136" y1="0" y2="720">
            <stop stopColor="#15515A" />
            <stop offset="1" stopColor="#24828B" />
          </linearGradient>
        </defs>
        <path
          d="M0 0H244C292 74 304 168 286 294C264 446 247 564 248 720H0V0Z"
          fill="url(#logo-parabola-fill)"
        />
      </svg>

      <Logo className="pointer-events-none absolute left-[-7%] top-[32%] z-0 h-[220px] w-auto opacity-[0.04] brightness-0 sm:left-[-4%] sm:h-[280px] md:left-[-3%] md:top-[33%] md:h-[34vh] lg:left-[2%] lg:top-[30%] lg:h-[44vh]" />

      <div className="relative z-10 flex min-h-[320px] flex-col px-8 pt-10 sm:min-h-[420px] sm:px-12 sm:pt-14 md:min-h-screen md:px-8 md:pt-16 lg:px-14 lg:pt-20 xl:px-20">
        <h1
          className={`${kenia.className} text-[48px] leading-[0.82] text-white sm:text-[58px] md:text-[54px] lg:text-[64px] xl:text-[74px]`}
        >
          Water
          <br />
          Path
        </h1>
        <p className="mt-6 max-w-[210px] text-xs font-bold leading-tight text-white sm:text-sm lg:mt-7">
          Monitoramento inteligente da qualidade da água
        </p>
      </div>
    </section>
  );
}


