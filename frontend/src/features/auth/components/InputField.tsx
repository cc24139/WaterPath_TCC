
import type { InputHTMLAttributes } from "react";

interface InputFieldProps extends InputHTMLAttributes<HTMLInputElement> {
    wrapperClassName?: string;
}

export function InputField({
    className = "",
    wrapperClassName = "",
    type = "text",
    ...props
}: InputFieldProps) {
    const border = "border border-[#DADADA] rounded-[5px] focus:border-[#DADADA] focus:outline-none focus:ring-0";
    const size = "w-full max-w-[460px]";
    const fontSize = "text-[12px] sm:text-[13px] md:text-[14px] lg:text-[15px]";

    return (
        <div className={`${size} ${wrapperClassName}`}>
            <input
                type={type}
                className={`w-full px-4 py-3 placeholder-placeholder ${border} ${fontSize} font-semibold text-text-primary ${className}`}
                {...props}
            />
        </div>
    );
}
