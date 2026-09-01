namespace VmcHmi.Application.Commands;

public class UnconfirmChecklistItemCommand
{
    public Guid SessionId { get; set; }
    public Guid ItemId { get; set; }
}
