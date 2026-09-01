using FluentValidation;
using VmcHmi.Application.Commands;

namespace VmcHmi.Application.Validators;

public class AdvanceStageCommandValidator : AbstractValidator<AdvanceStageCommand>
{
    public AdvanceStageCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
