import { StageHeader } from "../../components/StageHeader";
import { ChecklistItemCard } from "../../components/ChecklistItemCard";
import { PrimaryActionButton } from "../../components/PrimaryActionButton";
import { useSessionStore } from "../../state/sessionStore";

export const MachineChecksStage = () => {
  const { checklistItems, confirmItem, unconfirmItem, advanceStage, currentStage, setViewingStage, isLoading } = useSessionStore();
  const stageItems = checklistItems.filter((i) => i.stage === "MachineChecks");

  const confirmedCount = stageItems.filter((i) => i.isConfirmed).length;
  const isComplete = confirmedCount === stageItems.length && stageItems.length > 0;
  const progressPercent = stageItems.length > 0 ? Math.round((confirmedCount / stageItems.length) * 100) : 0;

  const handleAdvance = async () => {
    if (currentStage !== "MachineChecks") {
      setViewingStage("Tools");
    } else {
      await advanceStage();
    }
  };

  return (
    <div className="glass-stage">
      <StageHeader
        stage="MachineChecks"
        title="Machine Safety & Pre-Start Checks"
        subtitle="Verify emergency stops, fluid levels, guarding and operating pressure before tool setup."
      />

      {/* Progress & Stat Banner */}
      <div className="glass-banner">
        <div className="glass-banner__left">
          <div className="glass-banner__icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path strokeLinecap="round" strokeLinejoin="round" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
            </svg>
          </div>
          <div className="glass-banner__info">
            <span className="glass-banner__title">Safety Checklist Verification</span>
            <span className="glass-banner__desc">
              {confirmedCount} of {stageItems.length} safety points verified ({progressPercent}%)
            </span>
          </div>
        </div>

        <div className="glass-banner__right">
          <div className="glass-progress-ring-wrap">
            <span className={`glass-status-pill ${isComplete ? "glass-status-pill--ready" : ""}`}>
              {isComplete ? "ALL CHECKS CONFIRMED" : `${stageItems.length - confirmedCount} CHECKS REMAINING`}
            </span>
          </div>
        </div>
      </div>

      {/* Stage Checklist Items */}
      <div className="glass-checklist-section">
        <div className="glass-section-title">
          <span>Required Safety & Interlock Confirmations</span>
          <span className="count-badge">{confirmedCount}/{stageItems.length}</span>
        </div>

        <div className="glass-checklist">
          {stageItems.map((item) => (
            <ChecklistItemCard
              key={item.id}
              item={item}
              onConfirm={confirmItem}
              onUnconfirm={unconfirmItem}
            />
          ))}
        </div>
      </div>

      {/* Footer Action Bar */}
      <div className="glass-stage__footer">
        <div className="glass-stage__footer-hint">
          {isComplete ? (
            <span className="hint-success">✓ Machine checks passed. You may proceed to Tool Verification.</span>
          ) : (
            <span className="hint-pending">Confirm all items above to unlock Stage 02.</span>
          )}
        </div>
        <PrimaryActionButton
          label="Next: Tool Verification"
          onClick={handleAdvance}
          disabled={!isComplete || isLoading}
          variant="primary"
          icon="next"
        />
      </div>
    </div>
  );
};

