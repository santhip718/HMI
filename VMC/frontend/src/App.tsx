import { useEffect } from "react";
import type { ReactNode } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { useSessionStore } from "./state/sessionStore";
import { useAuth, LoginPage } from "./auth";
import { MachineChecksStage } from "./features/machineChecks/MachineChecksStage";
import { ToolsStage } from "./features/tools/ToolsStage";
import { WorkpieceStage } from "./features/workpiece/WorkpieceStage";
import { ReadyReviewStage } from "./features/readyReview/ReadyReviewStage";
import { OperationStage } from "./features/operation/OperationStage";
import type { StageType } from "./types";

import { Sidebar } from "./components/Sidebar";

function ProtectedLayout() {
  const { currentStage, fetchState, isLoading, error } = useSessionStore();

  useEffect(() => {
    fetchState();
  }, [fetchState]);

  if (isLoading && !currentStage) {
    return (
      <div className="glass-loading-screen">
        <div className="glass-spinner"></div>
        <p className="loading-text">Connecting to VMC CNC Controller...</p>
      </div>
    );
  }

  return (
    <div className="glass-layout">
      {/* Ambient background glow orbs */}
      <div className="glass-orb glass-orb--1"></div>
      <div className="glass-orb glass-orb--2"></div>
      <div className="glass-orb glass-orb--3"></div>

      <div className="glass-layout__inner">
        {/* Glass Sidebar */}
        <Sidebar />

        {/* Glass Main Canvas */}
        <main className="glass-main-canvas">
          {error && (
            <div className="glass-alert-error">
              <svg viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
              <span>{error}</span>
            </div>
          )}

          <div className="glass-stage-container">
            <AppContent />
          </div>
        </main>
      </div>
    </div>
  );
}

const STAGE_NAMES: Record<StageType, string> = {
  MachineChecks: "Stage 01: Machine Checks",
  Tools: "Stage 02: Tool Verification",
  Workpiece: "Stage 03: Workpiece Setup",
  ReadyReview: "Stage 04: Ready Review",
  Operation: "Stage 05: Live Operation",
};

function AppContent() {
  const { currentStage, viewingStage, setViewingStage } = useSessionStore();
  const activeStage = viewingStage || currentStage;

  const stageComponents: Record<StageType, ReactNode> = {
    MachineChecks: <MachineChecksStage />,
    Tools: <ToolsStage />,
    Workpiece: <WorkpieceStage />,
    ReadyReview: <ReadyReviewStage />,
    Operation: <OperationStage />,
  };

  const isReviewMode = activeStage !== currentStage;

  return (
    <>
      {isReviewMode && (
        <div className="glass-review-banner">
          <div className="glass-review-banner__left">
            <div className="glass-review-banner__icon">
              <svg viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="glass-review-banner__text">
              <span className="review-title">INSPECTION & REVIEW MODE</span>
              <span className="review-desc">
                Viewing <strong>{STAGE_NAMES[activeStage]}</strong>. All confirmed checks remain saved.
              </span>
            </div>
          </div>

          <button
            type="button"
            onClick={() => setViewingStage(currentStage)}
            className="glass-review-banner__btn"
          >
            <span>Return to Active Stage ({STAGE_NAMES[currentStage]?.split(":")[0]})</span>
            <svg viewBox="0 0 20 20" fill="currentColor">
              <path fillRule="evenodd" d="M10.293 3.293a1 1 0 011.414 0l6 6a1 1 0 010 1.414l-6 6a1 1 0 01-1.414-1.414L14.586 11H3a1 1 0 110-2h11.586l-4.293-4.293a1 1 0 010-1.414z" clipRule="evenodd" />
            </svg>
          </button>
        </div>
      )}
      {stageComponents[activeStage] || <MachineChecksStage />}
    </>
  );
}

function App() {
  const { isAuthenticated } = useAuth();

  return (
    <BrowserRouter>
      <Routes>
        <Route
          path="/login"
          element={isAuthenticated ? <Navigate to="/" replace /> : <LoginPage />}
        />
        <Route
          path="/"
          element={isAuthenticated ? <ProtectedLayout /> : <Navigate to="/login" replace />}
        />
        <Route
          path="*"
          element={<Navigate to={isAuthenticated ? "/" : "/login"} replace />}
        />
      </Routes>
    </BrowserRouter>
  );
}

export default App;

