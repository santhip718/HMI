import axios from "axios";

let rawUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.trim();

if (!rawUrl || rawUrl === "undefined" || rawUrl === "null") {
  if (typeof window !== "undefined" && window.location.hostname !== "localhost" && window.location.hostname !== "127.0.0.1") {
    rawUrl = "https://vmc-hmi-backend.onrender.com";
  } else {
    rawUrl = "http://localhost:5000";
  }
}

if (!rawUrl.startsWith("http://") && !rawUrl.startsWith("https://")) {
  rawUrl = `https://${rawUrl}`;
}

export const API_BASE_URL = rawUrl.replace(/\/+$/, "");

console.log("[HMI API Client] Connecting to backend at:", API_BASE_URL);

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const isLoginRequest = error.config?.url?.includes("/auth/login");
      if (!isLoginRequest) {
        localStorage.removeItem("token");
        window.dispatchEvent(new Event("auth:unauthorized"));
      }
    }
    return Promise.reject(error);
  },
);

