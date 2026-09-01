import { create } from "zustand";
import type { SessionState, StageType } from "../types";
import {
  getSessionState,
  confirmItem,
  unconfirmItem,
  advanceStage,
  startOperation,
  stopOperation,
  resetWorkflow as apiResetWorkflow,
} from "../api/session";

interface SessionStateStore extends SessionState {
  viewingStage: StageType;
  isLoading: boolean;
  error: string | null;
  setViewingStage: (stage: StageType) => void;
  fetchState: () => Promise<void>;
  confirmItem: (itemId: string) => Promise<void>;
  unconfirmItem: (itemId: string) => Promise<void>;
  advanceStage: () => Promise<void>;
  startOperation: () => Promise<void>;
  stopOperation: () => Promise<void>;
  resetWorkflow: () => Promise<void>;
  resetState: () => void;
}

const initialState: SessionState = {
  sessionId: "",
  currentStage: "MachineChecks",
  operationStatus: null,
  checklistItems: [],
  tools: [],
  operationRun: null,
};

export const useSessionStore = create<SessionStateStore>((set, get) => ({
  ...initialState,
  viewingStage: "MachineChecks",
  isLoading: false,
  error: null,

  setViewingStage: (stage: StageType) => {
    set({ viewingStage: stage });
  },

  resetState: () => set({ ...initialState, viewingStage: "MachineChecks", isLoading: false, error: null }),

  fetchState: async () => {
    set({ isLoading: true, error: null });
    try {
      const state = await getSessionState();
      const current = get().viewingStage;
      set({
        sessionId: state.sessionId,
        currentStage: state.currentStage,
        viewingStage: current || state.currentStage,
        operationStatus: state.operationStatus,
        checklistItems: state.checklistItems,
        tools: state.tools,
        operationRun: state.operationRun,
        isLoading: false,
      });
    } catch (err) {
      set({ error: (err as Error).message, isLoading: false });
    }
  },

  confirmItem: async (itemId: string) => {
    try {
      await confirmItem(itemId);
      const items = get().checklistItems.map((item) =>
        item.id === itemId ? { ...item, isConfirmed: true, confirmedAt: new Date().toISOString() } : item,
      );
      set({ checklistItems: items });
    } catch (err) {
      set({ error: (err as Error).message });
    }
  },

  unconfirmItem: async (itemId: string) => {
    try {
      await unconfirmItem(itemId);
      const items = get().checklistItems.map((item) =>
        item.id === itemId ? { ...item, isConfirmed: false, confirmedAt: null } : item,
      );
      set({ checklistItems: items });
    } catch (err) {
      set({ error: (err as Error).message });
    }
  },

  advanceStage: async () => {
    set({ isLoading: true, error: null });
    try {
      await advanceStage();
      const state = await getSessionState();
      set({
        sessionId: state.sessionId,
        currentStage: state.currentStage,
        viewingStage: state.currentStage,
        operationStatus: state.operationStatus,
        checklistItems: state.checklistItems,
        tools: state.tools,
        operationRun: state.operationRun,
        isLoading: false,
      });
    } catch (err) {
      set({ error: (err as Error).message, isLoading: false });
    }
  },

  startOperation: async () => {
    set({ isLoading: true, error: null });
    try {
      await startOperation();
      await get().fetchState();
    } catch (err) {
      set({ error: (err as Error).message, isLoading: false });
    }
  },

  stopOperation: async () => {
    set({ isLoading: true, error: null });
    try {
      await stopOperation();
      await get().fetchState();
    } catch (err) {
      set({ error: (err as Error).message, isLoading: false });
    }
  },

  resetWorkflow: async () => {
    set({ isLoading: true, error: null });
    try {
      const state = await apiResetWorkflow();
      set({
        sessionId: state.sessionId,
        currentStage: state.currentStage,
        viewingStage: state.currentStage,
        operationStatus: state.operationStatus,
        checklistItems: state.checklistItems,
        tools: state.tools,
        operationRun: state.operationRun,
        isLoading: false,
      });
    } catch (err) {
      set({ error: (err as Error).message, isLoading: false });
    }
  },
}));

