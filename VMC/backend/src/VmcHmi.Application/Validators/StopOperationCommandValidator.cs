using FluentValidation;
using VmcHmi.Application.Commands;

namespace VmcHmi.Application.Validators;

public class StopOperationCommandValidator : AbstractValidator<StopOperationCommand>
{
    public StopOperationCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
