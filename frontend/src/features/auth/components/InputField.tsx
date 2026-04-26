
interface InputFieldProps {
    placeholder: string
}
 
export function InputField({ placeholder }: InputFieldProps) {
    return (
        <div className="w-full max-w-[465px] mx-auto h-full max-h-[54px]">
            <input type="text" placeholder={placeholder} 
            className="placeholder-[#DADADA] placeholder:font-bold
            border border-[#DADADA] rounded-[5px] focus:border-[#DADADA]  focus:outline-none focus:ring-0  
            w-full h-full p-4 
            text-[16px] font-bold text-[#222222]" />
        </div>
    );
}   

