import type { StageType } from "../types";

interface StageHeaderProps {
  stage: StageType;
  title: string;
  subtitle?: string;
}

const STAGE_META: Record<StageType, { title: string; step: number; tag: string }> = {
  MachineChecks: { title: "Pre-Operation Machine Checks", step: 1, tag: "SAFETY & FLUIDICS" },
  Tools: { title: "Tool Loadout & Verification", step: 2, tag: "TOOL OFFSETS" },
  Workpiece: { title: "Workpiece & Clamping Setup", step: 3, tag: "FIXTURING & DATUM" },
  ReadyReview: { title: "Pre-Flight Job Review", step: 4, tag: "FINAL SIGN-OFF" },
  Operation: { title: "Live Machining Execution", step: 5, tag: "CNC RUNTIME" },
};

export const getStageTitle = (stage: StageType): string => STAGE_META[stage]?.title || stage;

export const StageHeader = ({ stage, title, subtitle }: StageHeaderProps) => {
  const meta = STAGE_META[stage] || { title, step: 1, tag: "STAGE" };

  return (
    <header className="glass-stage-header">
      <div className="glass-stage-header__left">
        <div className="glass-stage-header__badge-row">
          <span className="glass-stage-header__step-pill">STAGE 0{meta.step}</span>
          <span className="glass-stage-header__tag-pill">{meta.tag}</span>
        </div>
        <h1 className="glass-stage-header__title">{title || meta.title}</h1>
        {subtitle && <p className="glass-stage-header__subtitle">{subtitle}</p>}
      </div>
    </header>
  );
};

