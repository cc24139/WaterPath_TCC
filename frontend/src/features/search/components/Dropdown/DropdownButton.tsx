
import { FaChevronDown } from "react-icons/fa";

interface DropdownButtonProps{
    text: String
}

export function DropdownButton({text}: DropdownButtonProps){
    return(
        <div>
            {text}   
            <span>
                <FaChevronDown />
            </span>
        </div>
    ); // 100% feito a mão, 0% IA (indiano automatico)
}