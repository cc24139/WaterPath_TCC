
"use client";

import { useState, type InputHTMLAttributes } from "react";
import { LuEye, LuEyeOff } from "react-icons/lu";

interface InputFieldProps extends InputHTMLAttributes<HTMLInputElement> {
    wrapperClassName?: string;
}

export function InputField({
    className = "",
    wrapperClassName = "",
    type = "text",
    ...props
}: InputFieldProps) {
    const [isPasswordVisible, setIsPasswordVisible] = useState(false);
    const border = "border border-[#DADADA] rounded-[5px] focus:border-[#DADADA] focus:outline-none focus:ring-0";
    const size = "w-full max-w-[460px]";
    const fontSize = "text-[12px] sm:text-[13px] md:text-[14px] lg:text-[15px]";
    const isPasswordField = type === "password";
    const inputType = isPasswordField && isPasswordVisible ? "text" : type;
    const PasswordIcon = isPasswordVisible ? LuEye : LuEyeOff;

    return (
        <div className={`${size} ${wrapperClassName}`}>
          <div className="relative">
            <input
                type={inputType}
                className={`w-full px-4 py-3 placeholder-placeholder ${border} ${fontSize} font-semibold text-text-primary ${isPasswordField ? "pr-12" : ""} ${className}`}
                {...props}
            />
            {isPasswordField && (
                <button
                    type="button"
                    aria-label={isPasswordVisible ? "Ocultar senha" : "Mostrar senha"}
                    aria-pressed={isPasswordVisible}
                    onClick={() => setIsPasswordVisible((current) => !current)}
                    className="absolute right-3 top-1/2 inline-flex h-9 w-9 -translate-y-1/2 items-center justify-center rounded-md text-text-secondary transition-colors hover:text-primary focus:outline-none"
                >
                    <PasswordIcon className="h-5 w-5" />
                </button>
            )}
          </div>
        </div>
    );
}
