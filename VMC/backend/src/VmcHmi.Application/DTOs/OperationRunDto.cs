using VmcHmi.Domain.Enums;

namespace VmcHmi.Application.DTOs;

public class OperationRunDto
{
    public Guid Id { get; set; }
    public OperationStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }
}
