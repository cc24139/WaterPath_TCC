import { Logo } from "@/components/ui/Logo";

const footerLinks = [
  { label: "Sobre nós", href: "#" },
  { label: "Metodologias", href: "#" },
  { label: "Rios", href: "/water-bodies" },
];

export function Footer() {
  return (
    <footer className="border-t border-placeholder bg-background px-6 py-5 sm:px-10">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-5 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-2">
          <Logo className="h-8 w-auto" />
          <span className="font-heading text-[16px] font-bold text-primary">
            Water Path
          </span>
        </div>

        <nav aria-label="Links do rodapé">
          <ul className="flex flex-wrap gap-x-8 gap-y-3 text-[13px] text-text-secondary">
            {footerLinks.map((link) => (
              <li key={link.href}>
                <a href={link.href}>{link.label}</a>
              </li>
            ))}
          </ul>
        </nav>
      </div>
    </footer>
  );
}
