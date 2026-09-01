export type StageType =
  | "MachineChecks"
  | "Tools"
  | "Workpiece"
  | "ReadyReview"
  | "Operation";

export type OperationStatus = "Ready" | "Running" | "Stopped";

export type ChecklistStage = "MachineChecks" | "Tools" | "Workpiece";

export interface ChecklistItem {
  id: string;
  stage: ChecklistStage;
  label: string;
  sortOrder: number;
  isConfirmed: boolean;
  confirmedAt: string | null;
}

export interface Tool {
  id: string;
  code: string;
  description: string;
}

export interface OperationRun {
  id: string;
  status: OperationStatus;
  startedAt: string | null;
  stoppedAt: string | null;
}

export interface SessionState {
  sessionId: string;
  currentStage: StageType;
  operationStatus: OperationStatus | null;
  checklistItems: ChecklistItem[];
  tools: Tool[];
  operationRun: OperationRun | null;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
}
