import { useSessionStore } from "../state/sessionStore";
import { useAuth } from "../auth";
import type { StageType } from "../types";
import { useEffect, useState } from "react";

interface StageInfo {
  key: StageType;
  stepNumber: number;
  label: string;
  shortDesc: string;
  icon: string;
}

const STAGES: StageInfo[] = [
  {
    key: "MachineChecks",
    stepNumber: 1,
    label: "Machine Checks",
    shortDesc: "Safety & Fluidics",
    icon: "M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z",
  },
  {
    key: "Tools",
    stepNumber: 2,
    label: "Tool Verification",
    shortDesc: "Offsets & Loadout",
    icon: "M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z",
  },
  {
    key: "Workpiece",
    stepNumber: 3,
    label: "Workpiece Setup",
    shortDesc: "Material & Clamping",
    icon: "M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4",
  },
  {
    key: "ReadyReview",
    stepNumber: 4,
    label: "Ready Review",
    shortDesc: "Job Sign-off",
    icon: "M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4",
  },
  {
    key: "Operation",
    stepNumber: 5,
    label: "Operation",
    shortDesc: "Machining Run",
    icon: "M13 10V3L4 14h7v7l9-11h-7z",
  },
];

const STAGE_ORDER: Record<StageType, number> = {
  MachineChecks: 1,
  Tools: 2,
  Workpiece: 3,
  ReadyReview: 4,
  Operation: 5,
};

export const Sidebar = () => {
  const {
    currentStage,
    viewingStage,
    setViewingStage,
    checklistItems,
    operationStatus,
    resetWorkflow,
    isLoading,
  } = useSessionStore();
  const { logout } = useAuth();
  const [time, setTime] = useState<string>("");
  const [isResetConfirming, setIsResetConfirming] = useState(false);

  useEffect(() => {
    const updateTime = () => {
      const now = new Date();
      setTime(
        now.toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
          second: "2-digit",
          hour12: false,
        }),
      );
    };
    updateTime();
    const interval = setInterval(updateTime, 1000);
    return () => clearInterval(interval);
  }, []);

  const currentStep = STAGE_ORDER[currentStage] || 1;
  const activeViewingStage = viewingStage || currentStage;
  const progressPercent = Math.min(100, Math.round((currentStep / 5) * 100));

  const handleStageClick = (stageKey: StageType, isUnlocked: boolean) => {
    if (isUnlocked) {
      setViewingStage(stageKey);
    }
  };

  const handleResetWorkflow = async () => {
    if (!isResetConfirming) {
      setIsResetConfirming(true);
      return;
    }
    await resetWorkflow();
    setIsResetConfirming(false);
  };

  const getStageStatus = (stage: StageInfo) => {
    if (stage.stepNumber < currentStep) return "completed";
    if (stage.stepNumber === currentStep) return "active";
    return "pending";
  };

  const getStageSubtitle = (stage: StageInfo) => {
    if (stage.key === "MachineChecks") {
      const items = checklistItems.filter((i) => i.stage === "MachineChecks");
      const done = items.filter((i) => i.isConfirmed).length;
      return items.length > 0 ? `${done}/${items.length} checks` : "6 checks";
    }
    if (stage.key === "Tools") {
      const items = checklistItems.filter((i) => i.stage === "Tools");
      const done = items.filter((i) => i.isConfirmed).length;
      return items.length > 0 ? `${done}/${items.length} verified` : "5 checks";
    }
    if (stage.key === "Workpiece") {
      const items = checklistItems.filter((i) => i.stage === "Workpiece");
      const done = items.filter((i) => i.isConfirmed).length;
      return items.length > 0 ? `${done}/${items.length} confirmed` : "5 checks";
    }
    if (stage.key === "ReadyReview") {
      return stage.stepNumber < currentStep ? "Approved" : "Pre-flight";
    }
    if (stage.key === "Operation") {
      return operationStatus || "Ready to Run";
    }
    return stage.shortDesc;
  };

  return (
    <aside className="glass-sidebar">
      {/* Brand Header */}
      <div className="glass-sidebar__brand">
        <div className="glass-sidebar__logo-wrap">
          <div className="glass-sidebar__logo-glyph">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path strokeLinecap="round" strokeLinejoin="round" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z" />
            </svg>
          </div>
          <span className="glass-sidebar__logo-glow"></span>
        </div>
        <div className="glass-sidebar__brand-text">
          <h2 className="glass-sidebar__title">VMC HMI</h2>
          <span className="glass-sidebar__system-status">
            <span className="pulse-dot"></span> Online • Standby
          </span>
        </div>
      </div>

      {/* Progress Bar Card */}
      <div className="glass-sidebar__progress-card">
        <div className="glass-sidebar__progress-meta">
          <span className="glass-sidebar__progress-label">Workflow Progress</span>
          <span className="glass-sidebar__progress-val">{progressPercent}%</span>
        </div>
        <div className="glass-sidebar__progress-track">
          <div
            className="glass-sidebar__progress-fill"
            style={{ width: `${progressPercent}%` }}
          ></div>
        </div>
        <div className="glass-sidebar__progress-step-text">
          Stage {currentStep} of 5 — {STAGES[currentStep - 1]?.label}
        </div>
      </div>

      {/* Stepper Navigation */}
      <nav className="glass-sidebar__nav">
        <div className="glass-sidebar__nav-header">
          <span className="glass-sidebar__nav-title">WORKFLOW STAGES</span>
          <span className="glass-sidebar__nav-hint">Click to inspect</span>
        </div>
        <ul className="glass-sidebar__stepper">
          {STAGES.map((stage) => {
            const status = getStageStatus(stage);
            const isUnlocked = stage.stepNumber <= currentStep;
            const isViewingThis = stage.key === activeViewingStage;

            return (
              <li
                key={stage.key}
                onClick={() => handleStageClick(stage.key, isUnlocked)}
                className={`glass-sidebar__step glass-sidebar__step--${status} ${
                  isViewingThis ? "glass-sidebar__step--selected" : ""
                } ${isUnlocked ? "glass-sidebar__step--unlocked" : "glass-sidebar__step--locked"}`}
                title={
                  isUnlocked
                    ? `Click to view ${stage.label}`
                    : `Complete previous stages to unlock ${stage.label}`
                }
              >
                <div className="glass-sidebar__step-icon-wrap">
                  {status === "completed" ? (
                    <svg className="step-svg step-svg--check" viewBox="0 0 20 20" fill="currentColor">
                      <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                    </svg>
                  ) : (
                    <span className="step-num">{stage.stepNumber}</span>
                  )}
                </div>

                <div className="glass-sidebar__step-content">
                  <div className="glass-sidebar__step-heading">
                    <span className="glass-sidebar__step-title">{stage.label}</span>
                    {isViewingThis && stage.key !== currentStage && (
                      <span className="glass-sidebar__inspect-pill">INSPECT</span>
                    )}
                    {status === "active" && isViewingThis && (
                      <span className="glass-sidebar__active-pill">ACTIVE</span>
                    )}
                    {status === "completed" && !isViewingThis && (
                      <span className="glass-sidebar__done-pill">DONE</span>
                    )}
                  </div>
                  <span className="glass-sidebar__step-sub">
                    {getStageSubtitle(stage)}
                  </span>
                </div>
              </li>
            );
          })}
        </ul>

        {/* Workflow Reset Action */}
        <div className="glass-sidebar__reset-wrap">
          <button
            type="button"
            onClick={handleResetWorkflow}
            disabled={isLoading}
            className={`glass-sidebar__reset-btn ${isResetConfirming ? "glass-sidebar__reset-btn--confirming" : ""}`}
            title="Reset workflow to Stage 1"
          >
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path strokeLinecap="round" strokeLinejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            <span>{isResetConfirming ? "Confirm Reset to Stage 1?" : "Reset Workflow / New Run"}</span>
          </button>
          {isResetConfirming && (
            <button
              type="button"
              onClick={() => setIsResetConfirming(false)}
              className="glass-sidebar__reset-cancel"
            >
              Cancel
            </button>
          )}
        </div>
      </nav>

      {/* Job Context Widget */}
      <div className="glass-sidebar__job-card">
        <div className="glass-sidebar__job-header">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className="job-icon">
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z" />
          </svg>
          <span>ACTIVE JOB SCENARIO</span>
        </div>
        <div className="glass-sidebar__job-details">
          <div className="job-row">
            <span className="job-label">Part:</span>
            <span className="job-val">Al Mounting Bracket</span>
          </div>
          <div className="job-row">
            <span className="job-label">Program:</span>
            <span className="job-val highlight">O1042 Rev 3</span>
          </div>
          <div className="job-row">
            <span className="job-label">Work Offset:</span>
            <span className="job-val highlight">G54</span>
          </div>
          <div className="job-row">
            <span className="job-label">Fixture:</span>
            <span className="job-val">FX-118 (Vise 2)</span>
          </div>
        </div>
      </div>

      {/* Sidebar Footer */}
      <div className="glass-sidebar__footer">
        <div className="glass-sidebar__user-info">
          <div className="glass-sidebar__avatar">
            <span>OP</span>
          </div>
          <div className="glass-sidebar__user-meta">
            <span className="glass-sidebar__user-name">Operator</span>
            <span className="glass-sidebar__clock">{time || "00:00:00"}</span>
          </div>
        </div>
        <button
          onClick={logout}
          className="glass-sidebar__logout-btn"
          title="Sign Out of HMI"
        >
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path strokeLinecap="round" strokeLinejoin="round" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
          </svg>
          <span>Logout</span>
        </button>
      </div>
    </aside>
  );
};

