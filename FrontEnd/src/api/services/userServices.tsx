import { routes } from "../routes";
import { LoginDTO, UserRegisterDTO } from "../dtos/userDTO";
import { apiFetch } from "../apiFetch";

export const userServices = {
  async login(data: LoginDTO): Promise<Response> {
    return apiFetch(
      routes.user + "login",
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      },
      {
        authenticated: false,
        redirectOnUnauthorized: false,
      }
    );
  },

  async register(data: UserRegisterDTO): Promise<Response> {
    return apiFetch(
      routes.user + "cadastro",
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      },
      {
        authenticated: false,
        redirectOnUnauthorized: false,
      }
    );
  },
};
