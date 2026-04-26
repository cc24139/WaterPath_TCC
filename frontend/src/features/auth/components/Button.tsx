interface ButtonProps{
    text: string,
    variant: "primary" | "secondary"
}

export function Button({ text, variant }: ButtonProps){
    const size = "w-full h-full max-w-[180px] mx-auto max-h-[75px]"
    
    return(
            <button className={`${size} font-semibold text-[28px] rounded-[10px]
             ${variant === "primary"
                ? "bg-[#FF8A5B] text-[#FFFFFF]"
                : "bg-[#ffffff] text-[#FF8A5B] border border-[#DADADA]"
              }
            `}
            >
            {text}
            </button>
    );
}