interface PrimaryActionButtonProps {
  label: string;
  onClick: () => void;
  disabled?: boolean;
  variant?: "primary" | "danger" | "success";
  icon?: "next" | "play" | "stop" | "check";
}

export const PrimaryActionButton = ({
  label,
  onClick,
  disabled = false,
  variant = "primary",
  icon = "next",
}: PrimaryActionButtonProps) => {
  const btnClass = `glass-action-btn glass-action-btn--${variant}`;

  return (
    <button className={btnClass} onClick={onClick} disabled={disabled}>
      <span>{label}</span>
      {icon === "next" && (
        <svg viewBox="0 0 20 20" fill="currentColor" className="btn-icon">
          <path fillRule="evenodd" d="M10.293 3.293a1 1 0 011.414 0l6 6a1 1 0 010 1.414l-6 6a1 1 0 01-1.414-1.414L14.586 11H3a1 1 0 110-2h11.586l-4.293-4.293a1 1 0 010-1.414z" clipRule="evenodd" />
        </svg>
      )}
      {icon === "play" && (
        <svg viewBox="0 0 20 20" fill="currentColor" className="btn-icon">
          <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM9.555 7.168A1 1 0 008 8v4a1 1 0 001.555.832l3-2a1 1 0 000-1.664l-3-2z" clipRule="evenodd" />
        </svg>
      )}
      {icon === "stop" && (
        <svg viewBox="0 0 20 20" fill="currentColor" className="btn-icon">
          <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8 7a1 1 0 00-1 1v4a1 1 0 001 1h4a1 1 0 001-1V8a1 1 0 00-1-1H8z" clipRule="evenodd" />
        </svg>
      )}
      {icon === "check" && (
        <svg viewBox="0 0 20 20" fill="currentColor" className="btn-icon">
          <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
        </svg>
      )}
    </button>
  );
};

