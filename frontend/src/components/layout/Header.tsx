import { Logo } from "@/components/ui/Logo";

interface HeaderProps {
    variant?: 'default' | 'dark';
}

export function Header({ variant = 'default' }: HeaderProps) {
    const background = variant === 'dark' ? 'bg-[#0F2F35]' : 'bg-background';
    const text = variant === 'dark' ? 'text-[#FFFFFF]' : 'text-text-primary';
    
    return (
        <nav className={`flex flex-row justify-between items-center px-6 w-full h-16 shadow-[0_8px_15px_rgba(23,166,191,0.25)] ${background}`}>
            <div className="flex flex-row justify-between items-center gap-1">
                <Logo className="h-10 w-auto"/>
                <h1 className="font-bold text-primary text-[22px] font-heading">Water Path</h1>
            </div>

            <ul className="flex flex-row justify-between items-center gap-4 list-none">
                <li className={`text-[16px] ${text} font-light`}>Sobre nós</li>
                <li className={`text-[16px] ${text} font-light`}>Minhas Análises</li>
                <li className={`text-[16px] ${text} font-light`}>Metodologias</li>
            </ul> 
            
            <div className="flex flex-row justify-between items-center gap-2">
                <h1 className={`text-[16px] font-semibold text-contrast`}>Criar Conta</h1>
                <button className="px-4 py-1 rounded-lg bg-contrast text-white text-[16px] font-semibold shadow-[0_4px_8px_rgba(0,0,0,0.25)]">Comece agora</button>
            </div>
                
        </nav>
    );
}