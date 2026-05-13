import { CiSearch } from "react-icons/ci";
import { LuChevronDown } from "react-icons/lu";

export function RiverListToolBar() {
  const inputStyle =
    "placeholder:font-semibold placeholder:text-placeholder font-medium text-text-primary";
  const filterButtonClass =
    "h-7 lg:h-8 min-w-0 flex-1 sm:flex-none inline-flex items-center justify-center gap-1 px-3 lg:px-4 rounded-md border border-placeholder bg-white text-text-primary font-heading font-semibold text-[10px] lg:text-[11px] transition-colors hover:border-primary";

  return (
    <section
      className="
        bg-white max-w-[930px] lg:max-w-[1040px] xl:max-w-[1120px] mx-4 sm:mx-auto
        flex flex-col gap-3
        md:grid md:grid-cols-[minmax(150px,190px)_minmax(240px,1fr)_auto]
        lg:grid-cols-[minmax(180px,230px)_minmax(360px,1fr)_auto]
        md:items-center md:gap-5
        lg:gap-8
        px-4 py-4
        sm:px-5 lg:px-6
        rounded-xl sm:rounded-2xl shadow-default
      "
    >
      <div className="min-w-0">
        <div>
          <h1 className="font-heading text-text-secondary text-[20px] sm:text-[21px] lg:text-[23px] leading-tight">
            Rios Analisados
          </h1>

          <p className="font-heading text-[#767676] font-semibold text-[10px] sm:text-[11px] lg:text-[12px] leading-snug">
            Veja o status atual e previsões dos corpos hídricos
          </p>
        </div>
      </div>

      <div className="relative w-full min-w-0">
        <input
          type="text"
          placeholder="Pesquise os rios"
          className={`
            w-full h-8
            lg:h-9
            pl-4 pr-10
            outline-none
            border border-placeholder
            rounded-full
            bg-white
            text-[11px] lg:text-[12px]
            ${inputStyle}
          `}
        />

        <CiSearch
          size={18}
          className="
            text-text-primary
            absolute right-3 top-1/2 -translate-y-1/2
            pointer-events-none
          "
        />
      </div>

      <div className="flex flex-wrap items-center gap-2 sm:flex-nowrap">
        <div className="flex min-w-[180px] flex-1 gap-2 sm:min-w-fit sm:flex-none">
          <button className={filterButtonClass}>
            Status
            <LuChevronDown className="h-3 w-3 shrink-0" />
          </button>

          <button className={filterButtonClass}>
            Região
            <LuChevronDown className="h-3 w-3 shrink-0" />
          </button>
        </div>
        
        <section>
          <label className="flex h-8 flex-1 items-center justify-between gap-2 sm:h-auto sm:flex-none sm:flex-col sm:items-center sm:gap-0.5 cursor-pointer">
            <span className="font-heading text-[#767676] text-[9px] leading-none whitespace-nowrap">
              Minhas Análises
            </span>

            <input type="checkbox" defaultChecked className="peer sr-only" />

            <div
              className="
                w-8 h-4
                rounded-full
                bg-placeholder
                relative
                transition-colors
                peer-checked:bg-secondary
                after:content-['']
                after:absolute
                after:top-0.5
                after:left-0.5
                after:w-3
                after:h-3
                after:bg-white
                after:rounded-full
                after:transition-transform
                peer-checked:after:translate-x-4
              "
            />
          </label>
        </section>
      </div>
    </section>
  );
}
