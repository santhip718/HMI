import { StageHeader } from "../../components/StageHeader";
import { ChecklistItemCard } from "../../components/ChecklistItemCard";
import { PrimaryActionButton } from "../../components/PrimaryActionButton";
import { useSessionStore } from "../../state/sessionStore";

export const WorkpieceStage = () => {
  const { checklistItems, confirmItem, unconfirmItem, advanceStage, currentStage, setViewingStage, isLoading } = useSessionStore();
  const stageItems = checklistItems.filter((i) => i.stage === "Workpiece");

  const confirmedCount = stageItems.filter((i) => i.isConfirmed).length;
  const isComplete = confirmedCount === stageItems.length && stageItems.length > 0;

  const handleAdvance = async () => {
    if (currentStage !== "Workpiece") {
      setViewingStage("ReadyReview");
    } else {
      await advanceStage();
    }
  };

  return (
    <div className="glass-stage">
      <StageHeader
        stage="Workpiece"
        title="Workpiece Fixturing & Clamping Setup"
        subtitle="Verify raw stock material, datum face seating, clamping torque (25 Nm), and zero gap."
      />

      {/* Workpiece Specs Card */}
      <div className="glass-card-section">
        <div className="glass-section-title">
          <span>Part & Fixture Engineering Parameters</span>
          <span className="count-badge">SPECIFICATION SHEET</span>
        </div>

        <div className="glass-spec-grid">
          <div className="glass-spec-item">
            <span className="spec-label">RAW MATERIAL</span>
            <span className="spec-val">Aluminum 6061-T6</span>
          </div>
          <div className="glass-spec-item">
            <span className="spec-label">ORIENTATION</span>
            <span className="spec-val">Datum face to fixed jaw, pocket up</span>
          </div>
          <div className="glass-spec-item">
            <span className="spec-label">CLAMPING TORQUE</span>
            <span className="spec-val highlight-emerald">25 Nm (Torque Wrench)</span>
          </div>
          <div className="glass-spec-item">
            <span className="spec-label">GAP TOLERANCE</span>
            <span className="spec-val highlight-cyan">0.00 mm (Feeler Gauge Zero)</span>
          </div>
        </div>
      </div>

      {/* Stage Checklist Items */}
      <div className="glass-checklist-section">
        <div className="glass-section-title">
          <span>Workpiece Clamping Checks</span>
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
            <span className="hint-success">✓ Workpiece fixturing verified. Ready for Pre-flight Review.</span>
          ) : (
            <span className="hint-pending">Confirm all clamping points ({confirmedCount}/{stageItems.length}) to proceed.</span>
          )}
        </div>
        <PrimaryActionButton
          label="Next: Ready Review"
          onClick={handleAdvance}
          disabled={!isComplete || isLoading}
          variant="primary"
          icon="next"
        />
      </div>
    </div>
  );
};

