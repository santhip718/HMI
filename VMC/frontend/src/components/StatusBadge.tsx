import type { OperationStatus } from "../types";

interface StatusBadgeProps {
  status: OperationStatus;
  size?: "small" | "medium" | "large";
}

const STATUS_LABELS: Record<OperationStatus, string> = {
  Ready: "READY TO RUN",
  Running: "EXECUTION ACTIVE",
  Stopped: "OPERATION STOPPED",
};

export const StatusBadge = ({ status, size = "medium" }: StatusBadgeProps) => {
  const statusLower = (status || "ready").toLowerCase();

  return (
    <div className={`glass-status-badge glass-status-badge--${statusLower} glass-status-badge--${size}`}>
      <span className="glass-status-badge__dot"></span>
      <span className="glass-status-badge__text">{STATUS_LABELS[status] || status}</span>
    </div>
  );
};

