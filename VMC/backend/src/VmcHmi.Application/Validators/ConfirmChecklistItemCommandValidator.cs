using FluentValidation;
using VmcHmi.Application.Commands;

namespace VmcHmi.Application.Validators;

public class ConfirmChecklistItemCommandValidator : AbstractValidator<ConfirmChecklistItemCommand>
{
    public ConfirmChecklistItemCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
