const baseUrl = `${(process.env.NEXT_PUBLIC_API_URL ?? "https://waterpath-tcc.onrender.com/api").replace(/\/+$/, "")}/`;
export const routes = {
  user: `${baseUrl}user/`,
  codigo: `${baseUrl}codigo/`,
  corpoHidrico: `${baseUrl}corpohidrico/`,
  coleta: `${baseUrl}coleta/`,
  imagem: `${baseUrl}imagem/`,
  metalPesado: `${baseUrl}metalpesado/`,
  cianoBacteria: `${baseUrl}cianobacteria/`,
  qualidade: `${baseUrl}qualidade/`,
  qualidadeFutura: `${baseUrl}qualidadefutura/`,
  predicoes: `${baseUrl}ia/predicoes/`,
  vision: `${baseUrl}vision/`,
};
