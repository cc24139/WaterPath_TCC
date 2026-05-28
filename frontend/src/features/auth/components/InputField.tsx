
interface InputFieldProps {
    placeholder: string;
    type?: string;
}
 
export function InputField({ placeholder, type }: InputFieldProps) {
    
    const boarder = "border border-[#DADADA] rounded-[5px] focus:border-[#DADADA]  focus:outline-none focus:ring-0"
    
    const size = "w-full mx-auto max-w-[300px] sm:max-w-[400px] md:max-w-[460px] px-3 py-0.5 sm:px-4 sm:py-1 md:px-5 md:py-1.5"
    
    const fontsize = "text-[12px] sm:text-[14px] md:text-[16px]"


    return (
        <div className={`${size}`}>
            <input type={type} placeholder={placeholder} 
            className={`placeholder-placeholder
            ${boarder}  
            w-full h-full p-4 
            ${fontsize} font-bold text-text-primary`} />
        </div>
    );
}   

