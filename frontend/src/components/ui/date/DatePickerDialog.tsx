"use client";

import { useEffect, useId, useRef, type ReactNode, type RefObject } from "react";
import { LuCalendarDays, LuX } from "react-icons/lu";

export function DatePickerDialog({ anchorRef, title, onClose, children }: {
  anchorRef: RefObject<HTMLButtonElement | null>; title: string; onClose: () => void; children: ReactNode;
}) {
  const dialog = useRef<HTMLDialogElement>(null);
  const titleId = useId();
  useEffect(() => {
    const element = dialog.current;
    const anchor = anchorRef.current;
    if (!element) return;
    const position = () => {
      const mobile = window.innerWidth < 640;
      const width = Math.min(376, window.innerWidth - 24);
      const rect = anchor?.getBoundingClientRect();
      element.style.width = mobile ? "100%" : `${width}px`;
      element.style.left = mobile ? "0px" : `${Math.min(Math.max(12, rect?.left ?? 12), window.innerWidth - width - 12)}px`;
      element.style.top = mobile ? "auto" : `${Math.max(12, Math.min((rect?.bottom ?? 12) + 8, window.innerHeight - element.offsetHeight - 12))}px`;
      element.style.bottom = mobile ? "0px" : "auto";
    };
    element.showModal(); position();
    const overflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    window.addEventListener("resize", position);
    return () => {
      window.removeEventListener("resize", position);
      document.body.style.overflow = overflow;
      element.close();
      if (anchor?.isConnected) anchor.focus({ preventScroll: true });
    };
  }, [anchorRef]);
  return <dialog ref={dialog} aria-labelledby={titleId} className="wp-date-dialog fixed m-0 max-h-[90dvh] max-w-none overflow-y-auto overscroll-contain rounded-t-2xl border-0 bg-white p-0 text-text-primary shadow-default outline-none backdrop:bg-black/15 sm:max-h-[calc(100dvh-24px)] sm:rounded-xl sm:border sm:border-placeholder/60"
    onCancel={event => { event.preventDefault(); onClose(); }}
    onClick={event => { if (event.target === event.currentTarget) { const rect = event.currentTarget.getBoundingClientRect(); if (event.clientX < rect.left || event.clientX > rect.right || event.clientY < rect.top || event.clientY > rect.bottom) onClose(); } }}>
    <div className="p-4 sm:p-5">
      <div className="mx-auto mb-3 h-1 w-10 rounded-full bg-placeholder sm:hidden" />
      <header className="mb-4 flex items-center justify-between gap-3">
        <h2 id={titleId} className="flex items-center gap-2 font-heading text-sm font-bold"><LuCalendarDays aria-hidden="true" className="h-4 w-4 text-primary" />{title}</h2>
        <button type="button" onClick={onClose} aria-label="Fechar calendário" className="inline-flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-text-secondary hover:bg-primary/5 focus-visible:outline-primary"><LuX aria-hidden="true" className="h-4 w-4" /></button>
      </header>
      {children}
    </div>
  </dialog>;
}
