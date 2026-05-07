import { Logo } from "@/components/ui/Logo";
import { MdMenu, MdOutlineOpenInNew } from 'react-icons/md';

interface HeaderProps {
    variant?: 'default' | 'dark';
}

export function Header({ variant = 'default' }: HeaderProps) {
    const background = variant === 'dark' ? 'bg-[#0F2F35]' : 'bg-background';
    const text = variant === 'dark' ? 'text-[#FFFFFF]' : 'text-text-primary';
    
    // Falta criar o Drower para quando clicar nas 3 barrinhas
    return (
        <nav className={`flex flex-row justify-between gap-5 items-center px-3 sm:px-6 w-full h-16 shadow-[0_8px_15px_rgba(23,166,191,0.25)] ${background} `}>
            
            <div className="flex flex-row justify-between gap-2">
                <button className="sm:hidden">
                    <MdMenu className="h-6 w-auto text-primary" />
                </button>
                
                <div className="flex flex-row justify-between items-center gap-1">
                    <Logo className="h-7 sm:h-7 md:h-10 w-auto"/>
                    <h1 className="font-bold text-primary text-[18px] font-heading">Water Path</h1>
                </div>
            </div>

            <ul className="hidden sm:flex flex-row justify-between items-center sm:gap-2 md:gap-4 list-none">
                <li className={`md:text-[16px] sm:text-[12px] ${text} font-light`}>Sobre nós</li>
                <li className={`md:text-[16px] sm:text-[12px] ${text} font-light`}>Minhas Análises</li>
                <li className={`md:text-[16px] sm:text-[12px] ${text} font-light`}>Metodologias</li>
            </ul> 
            
            <div className="flex flex-row justify-between items-center gap-1.5 md:gap-2">
                <h1 className={`md:text-[16px] sm:text-[12px] text-[10px] font-semibold text-contrast`}>Criar Conta</h1>
                <button className="px-4 py-1 rounded-lg bg-contrast text-white text-[11px] sm:text-[12px] md:text-[16px] font-semibold shadow-[0_4px_8px_rgba(0,0,0,0.25)]">Comece agora</button>
            </div>

        </nav>
    );
}