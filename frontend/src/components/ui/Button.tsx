"use client";

import type { ButtonHTMLAttributes } from "react";
import { LuLoaderCircle } from "react-icons/lu";

type ButtonVariant = "primary" | "outline" | "ghost";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  isLoading?: boolean;
}

const variantClassNames: Record<ButtonVariant, string> = {
  primary:
    "border-primary bg-primary text-white shadow-[0_7px_18px_rgba(23,166,191,0.25)] hover:border-secondary hover:bg-secondary",
  outline:
    "border-placeholder bg-white text-text-primary hover:border-primary hover:text-primary",
  ghost:
    "border-transparent bg-transparent text-text-secondary hover:bg-primary/5 hover:text-primary",
};

export function Button({
  children,
  className = "",
  variant = "primary",
  isLoading = false,
  disabled,
  type = "button",
  ...props
}: ButtonProps) {
  return (
    <button
      type={type}
      disabled={disabled || isLoading}
      aria-busy={isLoading}
      className={`inline-flex h-12 items-center justify-center gap-2 rounded-md border px-5 font-heading text-[12px] font-bold transition-colors focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:cursor-not-allowed disabled:opacity-65 ${variantClassNames[variant]} ${className}`}
      {...props}
    >
      {isLoading ? <LuLoaderCircle className="h-4 w-4 animate-spin" /> : null}
      {children}
    </button>
  );
}

export const ButtonComponent = Button;

export default Button;
