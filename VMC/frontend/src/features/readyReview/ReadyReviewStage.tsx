import { StageHeader } from "../../components/StageHeader";
import { PrimaryActionButton } from "../../components/PrimaryActionButton";
import { useSessionStore } from "../../state/sessionStore";

export const ReadyReviewStage = () => {
  const { advanceStage, currentStage, setViewingStage, isLoading } = useSessionStore();

  const handleProceed = async () => {
    if (currentStage !== "ReadyReview") {
      setViewingStage("Operation");
    } else {
      await advanceStage();
    }
  };

  return (
    <div className="glass-stage">
      <StageHeader
        stage="ReadyReview"
        title="Pre-Flight Ready-to-Run Sign-Off"
        subtitle="All machine checks, tool loadouts, and workpiece fixtures have been confirmed. Verify parameters below before unlocking spindle start."
      />

      {/* Success Ready Banner */}
      <div className="glass-ready-hero">
        <div className="glass-ready-hero__glow"></div>
        <div className="glass-ready-hero__icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <div className="glass-ready-hero__text">
          <h2>Machine Setup Complete & Verified</h2>
          <p>System interlocks released. CNC controller is ready to arm the execution sequence.</p>
        </div>
      </div>

      {/* Job Execution Summary Card */}
      <div className="glass-card-section">
        <div className="glass-section-title">
          <span>Operation Execution Parameters</span>
          <span className="count-badge">JOB DOSSIER</span>
        </div>

        <div className="glass-summary-grid">
          <div className="glass-summary-card">
            <span className="summary-label">Target Operation</span>
            <span className="summary-val font-semibold">CNC Milling — Aluminum Bracket</span>
          </div>
          <div className="glass-summary-card">
            <span className="summary-label">Batch Quantity</span>
            <span className="summary-val highlight-cyan">25 Units</span>
          </div>
          <div className="glass-summary-card">
            <span className="summary-label">CNC Program & Revision</span>
            <span className="summary-val highlight-cyan">O1042, Rev 3</span>
          </div>
          <div className="glass-summary-card">
            <span className="summary-label">Work Coordinate System</span>
            <span className="summary-val highlight-emerald">G54 (X0 Y0 Z0 Verified)</span>
          </div>
          <div className="glass-summary-card">
            <span className="summary-label">Fixture Station</span>
            <span className="summary-val">FX-118 on Vise Station 2</span>
          </div>
          <div className="glass-summary-card">
            <span className="summary-label">Spindle State</span>
            <span className="summary-val highlight-emerald">Interlocks Cleared</span>
          </div>
        </div>
      </div>

      {/* Safety Gate Signoff Matrix */}
      <div className="glass-gate-matrix">
        <div className="gate-item gate-item--passed">
          <span className="gate-check">✓</span>
          <span>Stage 01: Machine & Safety Checks Confirmed</span>
        </div>
        <div className="gate-item gate-item--passed">
          <span className="gate-check">✓</span>
          <span>Stage 02: Tool Magazine & Offsets Verified</span>
        </div>
        <div className="gate-item gate-item--passed">
          <span className="gate-check">✓</span>
          <span>Stage 03: Workpiece Clamped & Torqued (25 Nm)</span>
        </div>
      </div>

      {/* Footer Action Bar */}
      <div className="glass-stage__footer">
        <div className="glass-stage__footer-hint">
          <span className="hint-success">Ready to transition into the live Machining Execution interface.</span>
        </div>
        <PrimaryActionButton
          label="Proceed to Live Machining"
          onClick={handleProceed}
          disabled={isLoading}
          variant="primary"
          icon="next"
        />
      </div>
    </div>
  );
};

