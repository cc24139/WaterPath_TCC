import { routes } from "../routes";
import { ImagemCadastroDTO } from "../dtos/imagemDTO";
import { apiFetch } from "../apiFetch";

export const imagemServices = {

  async create(data: ImagemCadastroDTO): Promise<Response> {
    const formData = new FormData();

     if (data.idCorpoHidrico)
       formData.append("IdCorpoHidrico", data.idCorpoHidrico.toString());
   
     if (data.idColeta)
       formData.append("idColeta", data.idColeta.toString());
   
     formData.append("DataUpload", data.dateTime.toISOString());
     formData.append("imagem", data.Imagem); 

     return apiFetch(routes.imagem, {
       method: "POST",
       body: formData,
     });
     },

  async getById(id: number): Promise<Response> {
    return apiFetch(`${routes.imagem}${id}`);
  },

  async getAll(): Promise<Response> {
    return apiFetch(routes.imagem);
  },

  async update(id: number, data: ImagemCadastroDTO): Promise<Response> {
    return apiFetch(`${routes.imagem}${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return apiFetch(`${routes.imagem}${id}`, {
      method: "DELETE",
    });
  },
};
