import { routes } from "../routes";
import { ImagemCadastroDTO } from "../dtos/imagemDTO";
import { getAuthHeaders } from "./authHeaders";

export const imagemServices = {

  async create(data: ImagemCadastroDTO): Promise<Response> {
    const formData = new FormData();

     if (data.idCorpoHidrico)
       formData.append("IdCorpoHidrico", data.idCorpoHidrico.toString());
   
     if (data.idColeta)
       formData.append("idColeta", data.idColeta.toString());
   
     formData.append("DataUpload", data.dateTime.toISOString());
     formData.append("imagem", data.Imagem); 

     return fetch(routes.imagem, {
       method: "POST",
       headers: getAuthHeaders(false), 
       body: formData,
     });
     },

  async getById(id: number): Promise<Response> {
    return fetch(`${routes.imagem}${id}`, {
      headers: getAuthHeaders(),
    });
  },

  async getAll(): Promise<Response> {
    return fetch(routes.imagem, {
      headers: getAuthHeaders(),
    });
  },

  async update(id: number, data: ImagemCadastroDTO): Promise<Response> {
    return fetch(`${routes.imagem}${id}`, {
      method: "PUT",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return fetch(`${routes.imagem}${id}`, {
      method: "DELETE",
      headers: getAuthHeaders(),
    });
  },
};
