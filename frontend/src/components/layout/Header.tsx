"use client";

import { useState } from "react";
import { MdClose, MdMenu } from "react-icons/md";

import { Logo } from "@/components/ui/Logo";

interface HeaderProps {
    variant?: "default" | "dark";
}

const headerLinks = [
    { label: "Sobre nós", href: "#" },
    { label: "Minhas Análises", href: "#" },
    { label: "Metodologias", href: "#" },
];

export function Header({ variant = "default" }: HeaderProps) {
    const [isDrawerOpen, setIsDrawerOpen] = useState(false);

    const background = variant === "dark" ? "bg-[#0F2F35]" : "bg-background";
    const text = variant === "dark" ? "text-[#FFFFFF]" : "text-text-primary";
    const drawerBorder = variant === "dark" ? "border-white/10" : "border-placeholder";

    function closeDrawer() {
        setIsDrawerOpen(false);
    }

    return (
        <>
            <nav className={`flex h-16 w-full flex-row items-center justify-between gap-5 px-3 shadow-[0_8px_15px_rgba(23,166,191,0.25)] sm:px-6 ${background}`}>
                <section className="flex flex-row items-center justify-between gap-2">
                    <button
                        type="button"
                        aria-label="Abrir menu"
                        aria-expanded={isDrawerOpen}
                        onClick={() => setIsDrawerOpen(true)}
                        className="inline-flex h-10 w-10 items-center justify-center text-primary transition-colors hover:text-secondary sm:hidden"
                    >
                        <MdMenu className="h-6 w-6" />
                    </button>

                    <div className="flex flex-row items-center justify-between gap-1">
                        <Logo className="h-7 w-auto sm:h-7 md:h-10" />
                        <h1 className="font-heading text-[18px] font-bold text-primary">Water Path</h1>
                    </div>
                </section>

                <section>
                    <HeaderLinkList
                        className="hidden flex-row items-center justify-between gap-2 sm:flex md:gap-4"
                        linkClassName={`text-[12px] font-light md:text-[16px] ${text}`}
                    />
                </section>

                <HeaderActions />
            </nav>

            {isDrawerOpen && (
                <button
                    type="button"
                    aria-label="Fechar menu"
                    onClick={closeDrawer}
                    className="fixed inset-0 z-40 bg-black/35 sm:hidden"
                />
            )}

            <aside
                className={`
                    fixed inset-y-0 left-0 z-50 flex min-h-screen w-[260px] flex-col border-r
                    px-5 py-4 shadow-[8px_0_24px_rgba(0,0,0,0.18)] transition-transform duration-300 sm:hidden
                    ${background} ${drawerBorder}
                    ${isDrawerOpen ? "translate-x-0" : "-translate-x-full"}
                `}
            >
                <button
                    type="button"
                    aria-label="Fechar menu"
                    onClick={closeDrawer}
                    className="absolute right-3 top-3 inline-flex h-9 w-9 items-center justify-center text-text-secondary transition-colors hover:text-primary"
                >
                    <MdClose className="h-6 w-6" />
                </button>

                <div className="flex items-center gap-2">
                    <Logo className="h-8 w-auto" />
                    <h2 className="font-heading text-[20px] font-bold text-primary">Water Path</h2>
                </div>

                <HeaderLinkList
                    onNavigate={closeDrawer}
                    className="mt-9 flex-col gap-5"
                    linkClassName={`font-heading text-[15px] font-medium ${text}`}
                />

                
            </aside>
        </>
    );
}

function HeaderLinkList({
    className,
    linkClassName,
    onNavigate,
}: {
    className: string;
    linkClassName: string;
    onNavigate?: () => void;
}) {
    return (
        <ul className={`flex list-none ${className}`}>
            {headerLinks.map((link) => (
                <li key={link.label}>
                    <a href={link.href} onClick={onNavigate} className={linkClassName}>
                        {link.label}
                    </a>
                </li>
            ))}
        </ul>
    );
}

function HeaderActions() {
    return (
        <section className="flex flex-row items-center justify-between gap-1.5 md:gap-2">
            <a href="#" className="text-[10px] font-semibold text-contrast sm:text-[12px] md:text-[16px]">
                Criar Conta
            </a>
            <button className="rounded-lg bg-contrast px-4 py-1 text-[11px] font-semibold text-white shadow-[0_4px_8px_rgba(0,0,0,0.25)] sm:text-[12px] md:text-[16px]">
                Comece agora
            </button>
        </section>
    );
}
