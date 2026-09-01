import { apiClient } from "./apiClient";
import type { LoginResponse } from "../types";

export const login = async (username: string, password: string): Promise<LoginResponse> => {
  try {
    const response = await apiClient.post<LoginResponse>("/api/auth/login", {
      username,
      password,
    });
    return response.data;
  } catch (err: any) {
    const message = err.response?.data?.message || err.message || "Invalid credentials";
    throw new Error(message);
  }
};

