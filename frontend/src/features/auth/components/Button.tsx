interface ButtonProps{
    text: string,
    variant: "primary" | "secondary"
}

export function Button({ text, variant }: ButtonProps){
    
    const size = "w-full mx-auto max-w-[100px] sm:max-w-[120px] md:max-w-[180px] px-3 py-0.5 sm:px-4 sm:py-1 md:px-5 md:py-1.5"
    
    const fontsize = "text-[18px] sm:text-[20px] md:text-[24px]"

    return(
            <button className={`${size} font-semibold ${fontsize} rounded-lg
             ${variant === "primary"
                ? "bg-contrast text-[#FFFFFF]"
                : "bg-transparent text-contrast border border-placeholder"
              }
            `}
            >
            {text}
            </button>
    );
}