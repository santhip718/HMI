import { StageHeader } from "../../components/StageHeader";
import { ChecklistItemCard } from "../../components/ChecklistItemCard";
import { PrimaryActionButton } from "../../components/PrimaryActionButton";
import { useSessionStore } from "../../state/sessionStore";

export const ToolsStage = () => {
  const { checklistItems, tools, confirmItem, unconfirmItem, advanceStage, currentStage, setViewingStage, isLoading } = useSessionStore();
  const stageItems = checklistItems.filter((i) => i.stage === "Tools");

  const confirmedCount = stageItems.filter((i) => i.isConfirmed).length;
  const isComplete = confirmedCount === stageItems.length && stageItems.length > 0;

  const handleAdvance = async () => {
    if (currentStage !== "Tools") {
      setViewingStage("Workpiece");
    } else {
      await advanceStage();
    }
  };

  return (
    <div className="glass-stage">
      <StageHeader
        stage="Tools"
        title="Tool Verification & Carousel Loadout"
        subtitle="Confirm physical tool installations, tool setter measurements, and G43 height offsets."
      />

      {/* Tools Inventory Grid */}
      <div className="glass-card-section">
        <div className="glass-section-title">
          <span>Required Tool Magazine Allocation</span>
          <span className="count-badge">{tools.length} TOOLS REQUIRED</span>
        </div>

        <div className="glass-tool-grid">
          {tools.map((tool, idx) => (
            <div key={tool.id} className="glass-tool-card">
              <div className="glass-tool-card__header">
                <span className="tool-code-pill">{tool.code}</span>
                <span className="tool-pocket-label">Pocket #{idx + 1}</span>
              </div>
              <div className="glass-tool-card__body">
                <h4 className="tool-name">{tool.description}</h4>
                <div className="tool-meta-tags">
                  <span className="meta-tag">H-Offset Active</span>
                  <span className="meta-tag">Magazine Locked</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Stage Checklist Items */}
      <div className="glass-checklist-section">
        <div className="glass-section-title">
          <span>Tool Loading & Measurement Sign-offs</span>
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
            <span className="hint-success">✓ All tools verified. You may proceed to Workpiece Setup.</span>
          ) : (
            <span className="hint-pending">Confirm all tool checks ({confirmedCount}/{stageItems.length}) to proceed.</span>
          )}
        </div>
        <PrimaryActionButton
          label="Next: Workpiece Setup"
          onClick={handleAdvance}
          disabled={!isComplete || isLoading}
          variant="primary"
          icon="next"
        />
      </div>
    </div>
  );
};

