using VmcHmi.Domain.Enums;

namespace VmcHmi.Domain.Entities;

public class ChecklistItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid SessionId { get; private set; } = Guid.Empty;
    public MachineSession? Session { get; private set; }
    public ChecklistStage Stage { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsConfirmed { get; private set; } = false;
    public DateTime? ConfirmedAt { get; private set; }

    private ChecklistItem() { }

    public ChecklistItem(Guid sessionId, ChecklistStage stage, string label, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new DomainException("Checklist item label cannot be empty.");

        SessionId = sessionId;
        Stage = stage;
        Label = label;
        SortOrder = sortOrder;
    }

    public void Confirm()
    {
        if (IsConfirmed)
            throw new DomainException("Checklist item is already confirmed.");
        IsConfirmed = true;
        ConfirmedAt = DateTime.UtcNow;
    }

    public void Unconfirm()
    {
        if (!IsConfirmed)
            throw new DomainException("Checklist item is not confirmed.");
        IsConfirmed = false;
        ConfirmedAt = null;
    }
}
