import { DropdownButton } from "./DropdownButton";

interface DropdownProps{
    buttonText: String
    content : 
}

export function Dropdown({buttonText, content } : DropdownProps){
    
    return(
        <div>
            <DropdownButton text={buttonText}/>
            
        </div>
    );
}