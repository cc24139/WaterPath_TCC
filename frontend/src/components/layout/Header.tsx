"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { LuBell, LuChevronDown } from "react-icons/lu";
import { MdClose, MdMenu } from "react-icons/md";

import { Logo } from "@/components/ui/Logo";

export interface HeaderUser {
    name: string;
    email: string;
    avatarUrl?: string;
}

export interface HeaderProps {
    variant?: "default" | "dark";
    isAuthenticated?: boolean;
    user?: HeaderUser;
    hasNotifications?: boolean;
}

type HeaderLink = {
    label: string;
    href: string;
};

const headerLinks: HeaderLink[] = [
    { label: "Sobre nós", href: "#" },
    { label: "Minhas análises", href: "/water-bodies" },
    { label: "Metodologias", href: "#" },
];

const fallbackUser: HeaderUser = {
    name: "Devlin",
    email: "devlin@email.com",
};

const primaryActionClass =
    "inline-flex min-h-10 items-center justify-center rounded-lg bg-contrast px-5 font-heading text-[13px] font-bold text-white shadow-default transition-colors hover:bg-contrast/90 md:text-[14px]";

export function Header({
    variant = "default",
    isAuthenticated,
    user,
    hasNotifications = true,
}: HeaderProps) {
    const [isDrawerOpen, setIsDrawerOpen] = useState(false);
    const [storedUser, setStoredUser] = useState<HeaderUser | null>(null);
    const pathname = usePathname();

    const background = variant === "dark" ? "bg-text-primary" : "bg-background";
    const text = variant === "dark" ? "text-background" : "text-text-primary";
    const mutedText = variant === "dark" ? "text-background/70" : "text-text-secondary";
    const drawerBorder = variant === "dark" ? "border-background/20" : "border-placeholder";
    const subtleHover = variant === "dark" ? "hover:bg-background/10" : "hover:bg-placeholder/30";
    const hoverText = variant === "dark" ? "hover:text-secondary" : "hover:text-primary";
    const separator = variant === "dark" ? "bg-background/20" : "bg-placeholder";
    const notificationDotBorder = variant === "dark" ? "border-text-primary" : "border-background";
    const isLoggedIn = isAuthenticated ?? Boolean(user || storedUser);
    const currentUser = user ?? storedUser ?? (isLoggedIn ? fallbackUser : null);

    useEffect(() => {
        const timeoutId = window.setTimeout(() => {
            if (user || isAuthenticated === false) {
                setStoredUser(null);
                return;
            }

            const token = localStorage.getItem("token");
            const storedName = localStorage.getItem("userName");
            const storedEmail = localStorage.getItem("userEmail");

            if (!token && !storedName && !storedEmail) {
                setStoredUser(null);
                return;
            }

            setStoredUser({
                name: storedName || fallbackUser.name,
                email: storedEmail || fallbackUser.email,
            });
        }, 0);

        return () => window.clearTimeout(timeoutId);
    }, [isAuthenticated, user]);

    function closeDrawer() {
        setIsDrawerOpen(false);
    }

    return (
        <>
            <nav className={`grid min-h-16 w-full grid-cols-[auto_minmax(0,1fr)] items-center gap-3 border-b ${drawerBorder} px-3 shadow-[0_8px_15px_rgba(23,166,191,0.25)] sm:px-6 md:grid-cols-[minmax(0,1fr)_auto_minmax(0,1fr)] lg:px-8 ${background}`}>
                <section className="flex min-w-0 flex-row items-center justify-self-start gap-2">
                    <button
                        type="button"
                        aria-label="Abrir menu"
                        aria-expanded={isDrawerOpen}
                        onClick={() => setIsDrawerOpen(true)}
                        className="inline-flex h-10 w-10 items-center justify-center text-primary transition-colors hover:text-secondary md:hidden"
                    >
                        <MdMenu className="h-6 w-6" />
                    </button>

                    <Link href="/" className="flex min-w-0 flex-row items-center gap-1">
                        <Logo className="h-7 w-auto sm:h-7 md:h-10" />
                        <h1 className="truncate font-heading text-[18px] font-bold text-primary">Water Path</h1>
                    </Link>
                </section>

                <section className="hidden justify-self-center md:block">
                    <HeaderLinkList
                        links={headerLinks}
                        className={`flex-row items-center justify-center gap-5 ${isLoggedIn ? "lg:gap-8" : "lg:gap-7"}`}
                        activePathname={pathname}
                        baseLinkClassName={`whitespace-nowrap ${isLoggedIn ? "font-heading text-[13px] font-medium lg:text-[15px]" : "text-[13px] font-light lg:text-[16px]"}`}
                        inactiveLinkClassName={text}
                    />
                </section>

                {isLoggedIn && currentUser ? (
                    <AuthenticatedHeaderActions
                        user={currentUser}
                        hasNotifications={hasNotifications}
                        textClassName={text}
                        mutedTextClassName={mutedText}
                        hoverClassName={`${subtleHover} ${hoverText}`}
                        separatorClassName={separator}
                        notificationDotBorderClassName={notificationDotBorder}
                    />
                ) : (
                    <PublicHeaderActions />
                )}
            </nav>

            {isDrawerOpen && (
                <button
                    type="button"
                    aria-label="Fechar menu"
                    onClick={closeDrawer}
                    className="fixed inset-0 z-40 bg-black/35 md:hidden"
                />
            )}

            <aside
                className={`
                    fixed inset-y-0 left-0 z-50 flex min-h-screen w-[260px] flex-col border-r
                    px-5 py-4 shadow-[8px_0_24px_rgba(0,0,0,0.18)] transition-transform duration-300 md:hidden
                    ${background} ${drawerBorder}
                    ${isDrawerOpen ? "translate-x-0" : "-translate-x-full"}
                `}
            >
                <button
                    type="button"
                    aria-label="Fechar menu"
                    onClick={closeDrawer}
                    className={`absolute right-3 top-3 inline-flex h-9 w-9 items-center justify-center transition-colors ${mutedText} ${hoverText}`}
                >
                    <MdClose className="h-6 w-6" />
                </button>

                <Link href="/" onClick={closeDrawer} className="flex items-center gap-2">
                    <Logo className="h-8 w-auto" />
                    <h2 className="font-heading text-[20px] font-bold text-primary">Water Path</h2>
                </Link>

                <HeaderLinkList
                    links={headerLinks}
                    onNavigate={closeDrawer}
                    className="mt-9 flex-col gap-5"
                    activePathname={pathname}
                    baseLinkClassName="font-heading text-[15px] font-medium"
                    inactiveLinkClassName={text}
                />

                {isLoggedIn && currentUser ? (
                    <AuthenticatedDrawerActions
                        user={currentUser}
                        hasNotifications={hasNotifications}
                        onNavigate={closeDrawer}
                        textClassName={text}
                        mutedTextClassName={mutedText}
                        borderClassName={drawerBorder}
                        hoverClassName={`${subtleHover} ${hoverText}`}
                        notificationDotBorderClassName={notificationDotBorder}
                    />
                ) : (
                    <PublicDrawerActions onNavigate={closeDrawer} />
                )}
            </aside>
        </>
    );
}

function HeaderLinkList({
    links,
    className,
    activePathname,
    baseLinkClassName,
    inactiveLinkClassName,
    onNavigate,
}: {
    links: HeaderLink[];
    className: string;
    activePathname: string;
    baseLinkClassName: string;
    inactiveLinkClassName: string;
    onNavigate?: () => void;
}) {
    return (
        <ul className={`flex list-none ${className}`}>
            {links.map((link) => {
                const isActive = isHeaderLinkActive(link.href, activePathname);

                return (
                    <li key={link.label}>
                        <a
                            href={link.href}
                            onClick={onNavigate}
                            aria-current={isActive ? "page" : undefined}
                            className={`${baseLinkClassName} transition-colors hover:text-secondary ${isActive ? "text-secondary" : inactiveLinkClassName}`}
                        >
                            {link.label}
                        </a>
                    </li>
                );
            })}
        </ul>
    );
}

function isHeaderLinkActive(href: string, pathname: string) {
    if (href === "#" || href.startsWith("#")) {
        return false;
    }

    if (href === "/") {
        return pathname === "/";
    }

    return pathname === href || pathname.startsWith(`${href}/`);
}

function PublicHeaderActions() {
    return (
        <section className="flex shrink-0 justify-self-end flex-row items-center justify-end gap-2">
            <a href="/register" className="hidden text-[12px] font-semibold text-contrast sm:inline md:text-[14px] lg:text-[16px]">
                Criar Conta
            </a>
            <Link
                href="/login"
                className="rounded-lg bg-contrast px-3 py-1.5 text-[11px] font-semibold text-white shadow-default transition-colors hover:bg-contrast/90 sm:px-4 sm:text-[12px] lg:text-[16px]"
            >
                Comece agora
            </Link>
        </section>
    );
}

function PublicDrawerActions({ onNavigate }: { onNavigate: () => void }) {
    return (
        <section className="mt-8 flex flex-col gap-3 border-t border-placeholder pt-5">
            <a
                href="#"
                onClick={onNavigate}
                className="font-heading text-[15px] font-semibold text-contrast"
            >
                Criar Conta
            </a>

            <Link href="/login" onClick={onNavigate} className={primaryActionClass}>
                Comece agora
            </Link>
        </section>
    );
}

function AuthenticatedHeaderActions({
    user,
    hasNotifications,
    textClassName,
    mutedTextClassName,
    hoverClassName,
    separatorClassName,
    notificationDotBorderClassName,
}: {
    user: HeaderUser;
    hasNotifications: boolean;
    textClassName: string;
    mutedTextClassName: string;
    hoverClassName: string;
    separatorClassName: string;
    notificationDotBorderClassName: string;
}) {
    return (
        <section className="flex min-w-0 shrink-0 justify-self-end flex-row items-center justify-end gap-1.5 sm:gap-2 lg:gap-3 xl:gap-4">
            <NotificationButton
                hasNotifications={hasNotifications}
                className="hidden sm:inline-flex"
                textClassName={textClassName}
                hoverClassName={hoverClassName}
                dotBorderClassName={notificationDotBorderClassName}
            />

            <button
                type="button"
                aria-label={`Abrir menu de ${user.name}`}
                aria-haspopup="menu"
                className={`inline-flex min-w-0 items-center gap-1.5 rounded-lg py-1 text-left transition-colors sm:gap-2 ${hoverClassName}`}
            >
                <UserAvatar user={user} />

                <span className="hidden min-w-0 flex-col leading-tight lg:flex">
                    <span className={`max-w-24 truncate font-heading text-[13px] font-bold xl:max-w-32 ${textClassName}`}>
                        {user.name}
                    </span>
                    <span className={`hidden max-w-36 truncate text-[11px] font-medium 2xl:block ${mutedTextClassName}`}>
                        {user.email}
                    </span>
                </span>

                <LuChevronDown className={`hidden h-4 w-4 shrink-0 sm:block ${mutedTextClassName}`} />
            </button>

            <span className={`hidden h-9 w-px xl:block ${separatorClassName}`} />

            <a href="/analysis" className={`${primaryActionClass} hidden xl:inline-flex`}>
                Nova análise
            </a>
        </section>
    );
}

function AuthenticatedDrawerActions({
    user,
    hasNotifications,
    onNavigate,
    textClassName,
    mutedTextClassName,
    borderClassName,
    hoverClassName,
    notificationDotBorderClassName,
}: {
    user: HeaderUser;
    hasNotifications: boolean;
    onNavigate: () => void;
    textClassName: string;
    mutedTextClassName: string;
    borderClassName: string;
    hoverClassName: string;
    notificationDotBorderClassName: string;
}) {
    return (
        <section className={`mt-8 border-t pt-5 ${borderClassName}`}>
            <div className="flex min-w-0 items-center gap-3">
                <UserAvatar user={user} />

                <div className="min-w-0">
                    <p className={`truncate font-heading text-[14px] font-bold ${textClassName}`}>
                        {user.name}
                    </p>
                    <p className={`truncate text-[12px] font-medium ${mutedTextClassName}`}>
                        {user.email}
                    </p>
                </div>
            </div>

            <div className="mt-5 flex items-center gap-3">
                <NotificationButton
                    hasNotifications={hasNotifications}
                    textClassName={textClassName}
                    hoverClassName={hoverClassName}
                    dotBorderClassName={notificationDotBorderClassName}
                />

                <a
                    href="/analysis"
                    onClick={onNavigate}
                    className={`${primaryActionClass} flex-1`}
                >
                    Nova análise
                </a>
            </div>
        </section>
    );
}

function NotificationButton({
    hasNotifications,
    className = "",
    textClassName = "text-text-primary",
    hoverClassName = "hover:bg-placeholder/30 hover:text-primary",
    dotBorderClassName = "border-background",
}: {
    hasNotifications: boolean;
    className?: string;
    textClassName?: string;
    hoverClassName?: string;
    dotBorderClassName?: string;
}) {
    return (
        <button
            type="button"
            aria-label="Abrir notificações"
            className={`relative inline-flex h-10 w-10 shrink-0 items-center justify-center rounded-full transition-colors ${textClassName} ${hoverClassName} ${className}`}
        >
            <LuBell className="h-5 w-5" />
            {hasNotifications && (
                <span className={`absolute right-2.5 top-2.5 h-2.5 w-2.5 rounded-full border-2 bg-contrast ${dotBorderClassName}`} />
            )}
        </button>
    );
}

function UserAvatar({ user }: { user: HeaderUser }) {
    const initials = getInitials(user.name);

    if (user.avatarUrl) {
        return (
            <Image
                src={user.avatarUrl}
                alt={`Avatar de ${user.name}`}
                width={40}
                height={40}
                unoptimized
                className="h-10 w-10 shrink-0 rounded-full object-cover"
            />
        );
    }

    return (
        <span className="inline-flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary font-heading text-[16px] font-bold text-white">
            {initials}
        </span>
    );
}

function getInitials(name: string) {
    const initials = name
        .trim()
        .split(" ")
        .filter(Boolean)
        .slice(0, 2)
        .map((part) => part[0])
        .join("")
        .toUpperCase();

    return initials || "U";
}
