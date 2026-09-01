import type { ChecklistItem } from "../types";

interface ChecklistItemCardProps {
  item: ChecklistItem;
  onConfirm: (itemId: string) => void;
  onUnconfirm: (itemId: string) => void;
}

export const ChecklistItemCard = ({ item, onConfirm, onUnconfirm }: ChecklistItemCardProps) => {
  return (
    <div className={`glass-check-item ${item.isConfirmed ? "glass-check-item--confirmed" : ""}`}>
      <div className="glass-check-item__indicator">
        {item.isConfirmed ? (
          <div className="glass-check-item__check-circle glass-check-item__check-circle--active">
            <svg viewBox="0 0 20 20" fill="currentColor">
              <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
            </svg>
          </div>
        ) : (
          <div className="glass-check-item__check-circle">
            <span className="glass-check-item__order">{item.sortOrder + 1}</span>
          </div>
        )}
      </div>

      <div className="glass-check-item__content">
        <span className="glass-check-item__label">{item.label}</span>
        {item.isConfirmed && item.confirmedAt && (
          <span className="glass-check-item__timestamp">
            <svg viewBox="0 0 20 20" fill="currentColor">
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-12a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V6z" clipRule="evenodd" />
            </svg>
            Confirmed at {new Date(item.confirmedAt).toLocaleTimeString()}
          </span>
        )}
      </div>

      <div className="glass-check-item__actions">
        {item.isConfirmed ? (
          <button
            className="glass-btn glass-btn--unconfirm"
            onClick={() => onUnconfirm(item.id)}
            title="Revoke confirmation"
          >
            <span>Revoke</span>
          </button>
        ) : (
          <button
            className="glass-btn glass-btn--confirm"
            onClick={() => onConfirm(item.id)}
            title="Mark as confirmed"
          >
            <svg viewBox="0 0 20 20" fill="currentColor">
              <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
            </svg>
            <span>Confirm</span>
          </button>
        )}
      </div>
    </div>
  );
};

