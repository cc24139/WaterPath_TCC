import { kenia } from "../../app/fonts";
import { Logo } from "./Logo";

export  function LogoParabola() {
    return (
      <div className={`text-2xl font-bold text-primary ${kenia.className}`}>
        <svg width="469" height="1032" viewBox="0 0 469 1032" fill="none" xmlns="http://www.w3.org/2000/svg">
            <g filter="url(#filter0_d_116_388)">
            <path d="M401 1024H4V0H401C592.762 439.673 279 700.501 401 1024Z" fill="url(#paint0_linear_116_388)"/>
            <path d="M400.672 0.5C496.328 220.045 465.869 394.952 426.675 555.447C387.524 715.768 339.6 861.842 400.278 1023.5H4.5V0.5H400.672Z" stroke="black"/>
            </g>
            <defs>
            <filter id="filter0_d_116_388" x="0" y="0" width="468.756" height="1032" filterUnits="userSpaceOnUse" color-interpolation-filters="sRGB">
            <feFlood flood-opacity="0" result="BackgroundImageFix"/>
            <feColorMatrix in="SourceAlpha" type="matrix" values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0" result="hardAlpha"/>
            <feOffset dy="4"/>
            <feGaussianBlur stdDeviation="2"/>
            <feComposite in2="hardAlpha" operator="out"/>
            <feColorMatrix type="matrix" values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.25 0"/>
            <feBlend mode="normal" in2="BackgroundImageFix" result="effect1_dropShadow_116_388"/>
            <feBlend mode="normal" in="SourceGraphic" in2="effect1_dropShadow_116_388" result="shape"/>
            </filter>
            <linearGradient id="paint0_linear_116_388" x1="234.378" y1="0" x2="234.378" y2="1024" gradientUnits="userSpaceOnUse">
            <stop stop-color="#0F2F35"/>
            <stop offset="1" stop-color="#2C899B"/>
            </linearGradient>
            </defs>  
            <text x="37%" y="10%" textAnchor="middle" fill="white" fontSize="70" dy=".3em">Water</text>
            <br/>
            <text x="37%" y="20%" textAnchor="middle" fill="white" fontSize="70" dy=".3em">Path</text>
            <text x="37%" y="30%" textAnchor="middle" fill="white" fontSize="25" dy=".3em">Monitoramento inteligente </text>
            <text x="37%" y="35%" textAnchor="middle" fill="white" fontSize="25" dy=".3em">da qualidade da água</text>
        </svg>
      </div>
    );
}

export default LogoParabola;