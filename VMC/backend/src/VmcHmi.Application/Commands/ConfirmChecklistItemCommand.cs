namespace VmcHmi.Application.Commands;

public class ConfirmChecklistItemCommand
{
    public Guid SessionId { get; set; }
    public Guid ItemId { get; set; }
}
