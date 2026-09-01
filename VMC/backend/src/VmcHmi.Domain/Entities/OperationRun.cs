using VmcHmi.Domain.Enums;

namespace VmcHmi.Domain.Entities;

public class OperationRun
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid SessionId { get; private set; } = Guid.Empty;
    public MachineSession? Session { get; private set; }
    public OperationStatus Status { get; private set; } = OperationStatus.Ready;
    public DateTime? StartedAt { get; private set; }
    public DateTime? StoppedAt { get; private set; }

    private OperationRun() { }

    public OperationRun(Guid sessionId)
    {
        SessionId = sessionId;
        Status = OperationStatus.Ready;
    }

    public void Start()
    {
        if (Status == OperationStatus.Running)
            throw new DomainException("Operation is already running.");
        Status = OperationStatus.Running;
        StartedAt = DateTime.UtcNow;
        StoppedAt = null;
    }

    public void Stop()
    {
        if (Status != OperationStatus.Running)
            throw new DomainException("Cannot stop an operation that is not running.");
        Status = OperationStatus.Stopped;
        StoppedAt = DateTime.UtcNow;
    }

    public void Reset()
    {
        Status = OperationStatus.Ready;
        StartedAt = null;
        StoppedAt = null;
    }
}
