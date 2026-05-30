import type { ButtonHTMLAttributes } from "react";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    text: string;
    variant: "primary" | "secondary";
}

export function Button({ text, variant, className = "", type = "button", ...props }: ButtonProps) {
    const size = "min-h-10 w-full max-w-[132px] px-4 py-2";

    const fontsize = "text-sm sm:text-base";

    return (
        <button
            type={type}
            className={`${size} font-semibold ${fontsize} rounded-lg shadow-sm transition-colors
             ${variant === "primary"
                ? "bg-contrast text-[#FFFFFF] hover:bg-[#e9784d]"
                : "bg-transparent text-contrast border border-placeholder hover:bg-placeholder/20"
            }
             ${className}
            `}
            {...props}
        >
            {text}
        </button>
    );
}
