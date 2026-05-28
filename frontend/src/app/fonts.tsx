import { Inter,Comic_Neue,Kenia }  from "next/font/google";

export const inter = Inter({
  subsets: ["latin"],
  variable: "--font-inter",
});
export const comicNeue = Comic_Neue({
    weight: ["400", "700"],
    subsets: ["latin"],
    variable: "--font-comic-neue",
});

export const kenia = Kenia({
    subsets: ["latin"],
    weight: ["400", "400"],
    variable: "--font-kenia",
})
