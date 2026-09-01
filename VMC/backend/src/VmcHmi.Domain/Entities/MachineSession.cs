using VmcHmi.Domain.Enums;

namespace VmcHmi.Domain.Entities;

public class MachineSession
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; } = Guid.Empty;
    public User? User { get; private set; }
    public StageType CurrentStage { get; private set; } = StageType.MachineChecks;
    public OperationStatus? OperationStatus { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private readonly List<ChecklistItem> _checklistItems = new();
    public IReadOnlyCollection<ChecklistItem> ChecklistItems => _checklistItems.AsReadOnly();

    private readonly List<Tool> _tools = new();
    public IReadOnlyCollection<Tool> Tools => _tools.AsReadOnly();

    public OperationRun? OperationRun { get; private set; }

    private MachineSession() { }

    public MachineSession(Guid userId)
    {
        UserId = userId;
        CurrentStage = StageType.MachineChecks;
        OperationRun = new OperationRun(Id);
    }

    public void AddChecklistItem(ChecklistStage stage, string label, int sortOrder)
    {
        _checklistItems.Add(new ChecklistItem(Id, stage, label, sortOrder));
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTool(string code, string description)
    {
        _tools.Add(new Tool(code, description));
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsStageComplete(ChecklistStage stage)
    {
        return _checklistItems
            .Where(i => i.Stage == stage)
            .All(i => i.IsConfirmed);
    }

    public bool CanAdvanceStage()
    {
        return CurrentStage switch
        {
            StageType.MachineChecks => IsStageComplete(ChecklistStage.MachineChecks),
            StageType.Tools => IsStageComplete(ChecklistStage.Tools),
            StageType.Workpiece => IsStageComplete(ChecklistStage.Workpiece),
            StageType.ReadyReview => true,
            _ => false
        };
    }

    public void AdvanceStage()
    {
        if (!CanAdvanceStage())
            throw new DomainException($"Cannot advance from stage {CurrentStage}. All items must be confirmed.");

        CurrentStage = CurrentStage switch
        {
            StageType.MachineChecks => StageType.Tools,
            StageType.Tools => StageType.Workpiece,
            StageType.Workpiece => StageType.ReadyReview,
            StageType.ReadyReview => StageType.Operation,
            _ => throw new DomainException("Cannot advance past the Operation stage.")
        };

        UpdatedAt = DateTime.UtcNow;
    }

    public void StartOperation()
    {
        if (CurrentStage != StageType.ReadyReview && CurrentStage != StageType.Operation)
            throw new DomainException("Operation can only start after ReadyReview stage.");

        OperationRun?.Start();
        OperationStatus = Enums.OperationStatus.Running;
        CurrentStage = StageType.Operation;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StopOperation()
    {
        if (OperationStatus != Enums.OperationStatus.Running)
            throw new DomainException("Operation must be running to stop it.");

        OperationRun?.Stop();
        OperationStatus = Enums.OperationStatus.Stopped;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmChecklistItem(Guid itemId)
    {
        var item = _checklistItems.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new DomainException("Checklist item not found.");
        item.Confirm();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnconfirmChecklistItem(Guid itemId)
    {
        var item = _checklistItems.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new DomainException("Checklist item not found.");
        item.Unconfirm();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetWorkflow()
    {
        CurrentStage = StageType.MachineChecks;
        OperationStatus = null;
        OperationRun = new OperationRun(Id);
        foreach (var item in _checklistItems)
        {
            if (item.IsConfirmed)
            {
                item.Unconfirm();
            }
        }
        UpdatedAt = DateTime.UtcNow;
    }
}
