
import { CiSearch } from "react-icons/ci";

export function RiverSearchBar(){
    var fontsInput = "placeholder:font-extrabold placeholder:text-[#E0E0E0] font-semibold text-text-primary"
    
    return(     
        <div className="bg-white flex flex-row justify-between items-center px-5 py-5 max-w-350 rounded-2xl shadow-default">
            
            <div>
                <h1 className="font-heading text-text-secondary text-[30px]">
                    Rios Analisados
                </h1>
                
                <p className="font-heading text-[#767676] font-semibold">
                    Veja o status atual e previsões dos <br></br>corpos hídricos
                </p>
            </div>

            <div className="relative w-full max-w-112.5">
                <input 
                type="text" 
                placeholder="Pesquise os rios"
                className={`w-full h-12  pl-5 pr-12 outline-none border border-placeholder rounded-full ${fontsInput}`}
                />
                <CiSearch size={22}  className="color-text-primary absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none" />
            </div>

            <div>

            </div>

        </div>

    );
}


//flex flex-row justify-between items-center border rounded-full border-placeholder px-4 py-1 w-lg