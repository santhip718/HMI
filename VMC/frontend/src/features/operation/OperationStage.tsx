import { useState } from "react";
import { StageHeader } from "../../components/StageHeader";
import { StatusBadge } from "../../components/StatusBadge";
import { PrimaryActionButton } from "../../components/PrimaryActionButton";
import { useSessionStore } from "../../state/sessionStore";
import type { OperationStatus } from "../../types";

export const OperationStage = () => {
  const { operationRun, operationStatus, startOperation, stopOperation, resetWorkflow, fetchState, isLoading } = useSessionStore();
  const [isResetConfirming, setIsResetConfirming] = useState(false);

  const handleStart = async () => {
    await startOperation();
    await fetchState();
  };

  const handleStop = async () => {
    await stopOperation();
    await fetchState();
  };

  const handleReset = async () => {
    if (!isResetConfirming) {
      setIsResetConfirming(true);
      return;
    }
    await resetWorkflow();
    setIsResetConfirming(false);
  };

  const currentStatus: OperationStatus = operationStatus || "Ready";

  return (
    <div className="glass-stage">
      <StageHeader
        stage="Operation"
        title="Live Machining Control Panel"
        subtitle="Real-time CNC execution interface for Aluminum Mounting Bracket (25 pcs batch)."
      />

      {/* Main Execution Console */}
      <div className={`glass-op-console glass-op-console--${currentStatus.toLowerCase()}`}>
        <div className="glass-op-console__top">
          <div className="op-info">
            <span className="op-prog-tag">CNC PROGRAM O1042 • REV 3</span>
            <h2 className="op-part-title">CNC Milling — Aluminum Mounting Bracket</h2>
            <p className="op-desc">Operation active on VMC 3-Axis Spindle • Work Offset G54 • Fixture FX-118</p>
          </div>

          <div className="op-badge-wrap">
            <StatusBadge status={currentStatus} size="large" />
          </div>
        </div>

        {/* Machine Telemetry Grid */}
        <div className="glass-telemetry-grid">
          <div className="telemetry-card">
            <div className="telemetry-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path strokeLinecap="round" strokeLinejoin="round" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div className="telemetry-data">
              <span className="telemetry-label">STARTED TIME</span>
              <span className="telemetry-value font-mono">
                {operationRun?.startedAt ? new Date(operationRun.startedAt).toLocaleTimeString() : "--:--:--"}
              </span>
            </div>
          </div>

          <div className="telemetry-card">
            <div className="telemetry-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path strokeLinecap="round" strokeLinejoin="round" d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z M9 10a1 1 0 011-1h4a1 1 0 011 1v4a1 1 0 01-1 1h-4a1 1 0 01-1-1v-4z" />
              </svg>
            </div>
            <div className="telemetry-data">
              <span className="telemetry-label">STOPPED TIME</span>
              <span className="telemetry-value font-mono">
                {operationRun?.stoppedAt ? new Date(operationRun.stoppedAt).toLocaleTimeString() : "--:--:--"}
              </span>
            </div>
          </div>

          <div className="telemetry-card">
            <div className="telemetry-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path strokeLinecap="round" strokeLinejoin="round" d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
            </div>
            <div className="telemetry-data">
              <span className="telemetry-label">SPINDLE SPEED</span>
              <span className={`telemetry-value font-mono ${currentStatus === "Running" ? "highlight-emerald" : ""}`}>
                {currentStatus === "Running" ? "4,500 RPM" : "0 RPM (IDLE)"}
              </span>
            </div>
          </div>

          <div className="telemetry-card">
            <div className="telemetry-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path strokeLinecap="round" strokeLinejoin="round" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
              </svg>
            </div>
            <div className="telemetry-data">
              <span className="telemetry-label">FEED RATE</span>
              <span className={`telemetry-value font-mono ${currentStatus === "Running" ? "highlight-cyan" : ""}`}>
                {currentStatus === "Running" ? "800 mm/min" : "0 mm/min"}
              </span>
            </div>
          </div>
        </div>

        {/* Live Controller Actions Bar */}
        <div className="glass-op-actions">
          {currentStatus !== "Running" ? (
            <div className="op-action-container">
              <PrimaryActionButton
                label={isLoading ? "Starting Spindle..." : "Start Machining Operation"}
                onClick={handleStart}
                disabled={isLoading}
                variant="primary"
                icon="play"
              />
              <span className="op-action-hint">Pressing Start engages the G-code sequence for Part #01 of 25.</span>
            </div>
          ) : (
            <div className="op-action-container">
              <PrimaryActionButton
                label={isLoading ? "Stopping..." : "EMERGENCY / CYCLE STOP"}
                onClick={handleStop}
                disabled={isLoading}
                variant="danger"
                icon="stop"
              />
              <span className="op-action-hint op-action-hint--warn">Machining in progress. Press to safely halt axis feed and spindle.</span>
            </div>
          )}
        </div>

        {/* Workflow Cycle Complete / Reset Section */}
        {currentStatus !== "Running" && (
          <div className="glass-op-reset-section">
            <div className="reset-info">
              <span className="reset-title">Batch Job Management</span>
              <span className="reset-desc">Finished machining cycle or want to repeat setup from Stage 01?</span>
            </div>
            <div className="reset-actions">
              <button
                type="button"
                onClick={handleReset}
                disabled={isLoading}
                className={`glass-btn-workflow-reset ${isResetConfirming ? "glass-btn-workflow-reset--confirm" : ""}`}
              >
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
                <span>{isResetConfirming ? "Confirm & Reset Workflow to Stage 01" : "Start New Setup Run (Reset All Checks)"}</span>
              </button>
              {isResetConfirming && (
                <button
                  type="button"
                  onClick={() => setIsResetConfirming(false)}
                  className="glass-btn-cancel"
                >
                  Cancel
                </button>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};


