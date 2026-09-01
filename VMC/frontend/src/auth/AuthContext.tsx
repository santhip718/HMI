import { createContext, useContext, useState, useEffect } from "react";
import type { ReactNode } from "react";
import { login as loginApi } from "../api/auth";
import { useSessionStore } from "../state/sessionStore";

interface AuthContextType {
  token: string | null;
  isAuthenticated: boolean;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within an AuthProvider");
  return context;
};

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem("token"));

  useEffect(() => {
    const handleUnauthorized = () => {
      localStorage.removeItem("token");
      useSessionStore.getState().resetState();
      setToken(null);
    };

    window.addEventListener("auth:unauthorized", handleUnauthorized);
    return () => {
      window.removeEventListener("auth:unauthorized", handleUnauthorized);
    };
  }, []);

  const handleLogin = async (username: string, password: string) => {
    const response = await loginApi(username, password);
    localStorage.setItem("token", response.token);
    setToken(response.token);
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    useSessionStore.getState().resetState();
    setToken(null);
  };

  return (
    <AuthContext.Provider
      value={{
        token,
        isAuthenticated: !!token,
        login: handleLogin,
        logout: handleLogout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

