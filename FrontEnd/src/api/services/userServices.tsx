import {routes} from "../routes";
import {LoginDTO, LoginResponseDTO,} from "../dtos/userDTO";

export const userServices = {

  async login(data: LoginDTO): Promise<Response> {
    const response = await fetch(routes.user + "login", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });
    return response;
  }
};
