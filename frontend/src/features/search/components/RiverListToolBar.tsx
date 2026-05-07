import { CiSearch } from "react-icons/ci";

export function RiverListToolBar() {
  const inputStyle =
    "placeholder:font-extrabold placeholder:text-placeholder font-semibold text-text-primary";

  return (
    <section
      className="
        bg-white w-full max-w-[1400px] mx-auto
        flex flex-col gap-3
        sm:gap-4
        lg:flex-row lg:items-center lg:justify-between
        px-3 py-3
        sm:px-5 sm:py-4
        lg:py-5
        rounded-xl sm:rounded-2xl shadow-default
      "
    >
      <div className="lg:min-w-[220px]">
        <h1 className="font-heading text-text-secondary text-[22px] sm:text-[26px] lg:text-[30px] leading-tight">
          Rios Analisados
        </h1>

        <p className="font-heading text-[#767676] font-semibold text-[12px] sm:text-[14px] lg:text-[15px] leading-snug">
          Veja o status atual e previsões dos
          <br className="hidden sm:block" /> corpos hídricos
        </p>
      </div>

      <div className="relative w-full lg:max-w-[450px]">
        <input
          type="text"
          placeholder="Pesquise os rios"
          className={`
            w-full h-10
            sm:h-12
            pl-4 pr-10
            sm:pl-5 sm:pr-12
            outline-none
            border border-placeholder
            rounded-full
            bg-white
            text-[13px] sm:text-[14px]
            ${inputStyle}
          `}
        />

        <CiSearch
          size={20}
          className="
            text-text-primary
            absolute right-3 sm:right-4 top-1/2 -translate-y-1/2
            pointer-events-none
          "
        />
      </div>

      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:gap-3">
        <div className="flex gap-2 sm:gap-3">
          <button
            className="
              h-8 flex-1 px-3
              sm:h-9 sm:flex-none sm:px-5
              rounded-lg
              border border-placeholder
              bg-white
              text-text-primary
              font-heading font-semibold text-[12px] sm:text-[14px]
            "
          >
            Status ▼
          </button>

          <button
            className="
              h-8 flex-1 px-3
              sm:h-9 sm:flex-none sm:px-5
              rounded-lg
              border border-placeholder
              bg-white
              text-text-primary
              font-heading font-semibold text-[12px] sm:text-[14px]
            "
          >
            Região ▼
          </button>
        </div>

        <label className="flex items-center justify-between gap-2 cursor-pointer sm:justify-start">
          <span className="font-heading text-[#767676] text-[10px] sm:text-[11px]">
            Minhas Análises
          </span>

          <input type="checkbox" className="peer sr-only" />

          <div
            className="
              w-10 h-5
              sm:w-12 sm:h-6
              rounded-full
              bg-placeholder
              relative
              transition-colors
              peer-checked:bg-secondary
              after:content-['']
              after:absolute
              after:top-1
              after:left-1
              after:w-3
              after:h-3
              sm:after:w-4
              sm:after:h-4
              after:bg-white
              after:rounded-full
              after:transition-transform
              peer-checked:after:translate-x-5
              sm:peer-checked:after:translate-x-6
            "
          />
        </label>
      </div>
    </section>
  );
}
