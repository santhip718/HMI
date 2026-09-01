using VmcHmi.Domain.Enums;

namespace VmcHmi.Application.DTOs;

public class ChecklistItemDto
{
    public Guid Id { get; set; }
    public string Stage { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}
