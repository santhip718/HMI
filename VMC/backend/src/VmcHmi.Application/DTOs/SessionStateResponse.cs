using VmcHmi.Domain.Enums;

namespace VmcHmi.Application.DTOs;

public class SessionStateResponse
{
    public Guid SessionId { get; set; }
    public StageType CurrentStage { get; set; }
    public OperationStatus? OperationStatus { get; set; }
    public List<ChecklistItemDto> ChecklistItems { get; set; } = new();
    public List<ToolDto> Tools { get; set; } = new();
    public OperationRunDto? OperationRun { get; set; }
}
