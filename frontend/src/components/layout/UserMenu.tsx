"use client";

import { useEffect, useId, useRef, useState, type KeyboardEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { LuChartNoAxesCombined, LuChevronDown, LuDroplet, LuLogOut, LuUserRound } from "react-icons/lu";

import { Card } from "@/components/ui/Card";
import { UserAvatar } from "@/components/ui/UserAvatar";
import { clearAuthSession } from "@/features/auth/utils/authSession";
import type { HeaderUser } from "./Header";

interface UserMenuProps {
  user: HeaderUser;
  textClassName: string;
  mutedTextClassName: string;
  hoverClassName: string;
  // Supply the profile route here once that page exists.
  profileHref?: string;
}

const itemClassName = "flex min-h-11 w-full items-center gap-3 rounded-lg px-3 py-2.5 text-left font-heading text-sm font-medium text-text-primary transition-colors hover:bg-primary/5 hover:text-primary focus-visible:bg-primary/5 focus-visible:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30";

export function UserMenu({ user, textClassName, mutedTextClassName, hoverClassName, profileHref }: UserMenuProps) {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const triggerRef = useRef<HTMLButtonElement>(null);
  const menuRef = useRef<HTMLDivElement>(null);
  const focusLastItem = useRef(false);
  const menuId = useId();
  const router = useRouter();

  useEffect(() => {
    if (!isOpen) return;

    const items = menuRef.current?.querySelectorAll<HTMLElement>('[role="menuitem"]');
    const index = focusLastItem.current ? (items?.length ?? 1) - 1 : 0;
    items?.[index]?.focus();

    function closeOutside(event: Event) {
      if (event.target instanceof Node && !containerRef.current?.contains(event.target)) {
        setIsOpen(false);
      }
    }

    function closeOnEscape(event: globalThis.KeyboardEvent) {
      if (event.key === "Escape") {
        event.preventDefault();
        setIsOpen(false);
        triggerRef.current?.focus();
      }
    }

    document.addEventListener("pointerdown", closeOutside);
    document.addEventListener("focusin", closeOutside);
    document.addEventListener("keydown", closeOnEscape);
    return () => {
      document.removeEventListener("pointerdown", closeOutside);
      document.removeEventListener("focusin", closeOutside);
      document.removeEventListener("keydown", closeOnEscape);
    };
  }, [isOpen]);

  function handleMenuKeyDown(event: KeyboardEvent<HTMLDivElement>) {
    if (event.key === "Tab") {
      setIsOpen(false);
      triggerRef.current?.focus();
      return;
    }

    if (!["ArrowDown", "ArrowUp", "Home", "End"].includes(event.key)) return;
    event.preventDefault();

    const items = Array.from(menuRef.current?.querySelectorAll<HTMLElement>('[role="menuitem"]') ?? []);
    const currentIndex = items.findIndex((item) => item === document.activeElement);
    let nextIndex = currentIndex;
    if (event.key === "ArrowDown") nextIndex = (currentIndex + 1) % items.length;
    if (event.key === "ArrowUp") nextIndex = (currentIndex - 1 + items.length) % items.length;
    if (event.key === "Home") nextIndex = 0;
    if (event.key === "End") nextIndex = items.length - 1;
    items[nextIndex]?.focus();
  }

  function closeMenu() {
    setIsOpen(false);
    triggerRef.current?.focus();
  }

  function handleLogout() {
    closeMenu();
    clearAuthSession();
    router.replace("/login");
    router.refresh();
  }

  const links = [
    { label: "Meu perfil", href: profileHref, icon: LuUserRound },
    { label: "Minhas análises", href: "/my-analyses", icon: LuChartNoAxesCombined },
    { label: "Adicionar corpo hídrico", href: "/add-water-bodie", icon: LuDroplet },
  ];

  return (
    <div ref={containerRef} className="relative min-w-0">
      <button
        ref={triggerRef}
        type="button"
        id={`${menuId}-trigger`}
        aria-label={`Menu de ${user.name}`}
        aria-haspopup="menu"
        aria-expanded={isOpen}
        aria-controls={isOpen ? menuId : undefined}
        onClick={() => {
          focusLastItem.current = false;
          setIsOpen((open) => !open);
        }}
        onKeyDown={(event) => {
          if (event.key === "ArrowDown" || event.key === "ArrowUp") {
            event.preventDefault();
            focusLastItem.current = event.key === "ArrowUp";
            setIsOpen(true);
          }
        }}
        className={`inline-flex min-w-0 items-center gap-1.5 rounded-lg py-1 text-left transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary sm:gap-2 ${hoverClassName}`}
      >
        <UserAvatar user={user} />
        <span className="hidden min-w-0 flex-col leading-tight lg:flex">
          <span className={`max-w-24 truncate font-heading text-[13px] font-bold xl:max-w-32 ${textClassName}`}>{user.name}</span>
          <span className={`hidden max-w-36 truncate text-[11px] font-medium 2xl:block ${mutedTextClassName}`}>{user.email}</span>
        </span>
        <LuChevronDown aria-hidden="true" className={`hidden h-4 w-4 shrink-0 transition-transform motion-reduce:transition-none sm:block ${isOpen ? "rotate-180" : ""} ${mutedTextClassName}`} />
      </button>

      {isOpen && (
        <Card className="absolute right-0 top-full z-30 mt-2 max-h-[calc(100dvh-5rem)] w-80 max-w-[calc(100vw-1.5rem)] overflow-y-auto border border-placeholder/60">
          <div className="flex min-w-0 items-center gap-3 border-b border-placeholder/60 pb-4">
            <UserAvatar user={user} />
            <div className="min-w-0">
              <p className="break-words font-heading text-base font-bold text-text-primary">{user.name}</p>
              <p className="break-all text-sm text-text-secondary">{user.email}</p>
            </div>
          </div>
          <div ref={menuRef} id={menuId} role="menu" aria-labelledby={`${menuId}-trigger`} onKeyDown={handleMenuKeyDown} className="pt-2">
            {links.map(({ label, href, icon: Icon }) => href ? (
              <Link key={label} href={href} role="menuitem" tabIndex={-1} onClick={closeMenu} className={itemClassName}>
                <Icon aria-hidden="true" className="h-5 w-5 shrink-0" />
                {label}
              </Link>
            ) : (
              <button key={label} type="button" role="menuitem" tabIndex={-1} aria-disabled="true" aria-label={`${label} (em breve)`} title="Meu perfil estará disponível em breve" className={`${itemClassName} cursor-not-allowed opacity-50`}>
                <Icon aria-hidden="true" className="h-5 w-5 shrink-0" />
                {label}
              </button>
            ))}
            <div role="separator" className="my-2 border-t border-placeholder/60" />
            <button type="button" role="menuitem" tabIndex={-1} onClick={handleLogout} className={itemClassName}>
              <LuLogOut aria-hidden="true" className="h-5 w-5 shrink-0" />
              Sair
            </button>
          </div>
        </Card>
      )}
    </div>
  );
}
