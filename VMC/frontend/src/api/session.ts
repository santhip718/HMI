import { apiClient } from "./apiClient";
import type { SessionState } from "../types";

export const getSessionState = async (): Promise<SessionState> => {
  const response = await apiClient.get("/api/session/current");
  return response.data;
};

export const confirmItem = async (itemId: string): Promise<void> => {
  await apiClient.post(`/api/session/checklist/${itemId}/confirm`);
};

export const unconfirmItem = async (itemId: string): Promise<void> => {
  await apiClient.post(`/api/session/checklist/${itemId}/unconfirm`);
};

export const advanceStage = async (): Promise<void> => {
  await apiClient.post("/api/session/advance");
};

export const startOperation = async (): Promise<void> => {
  await apiClient.post("/api/session/operation/start");
};

export const stopOperation = async (): Promise<void> => {
  await apiClient.post("/api/session/operation/stop");
};

export const resetWorkflow = async (): Promise<SessionState> => {
  const response = await apiClient.post<SessionState>("/api/session/reset");
  return response.data;
};

