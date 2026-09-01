using FluentValidation;
using VmcHmi.Application.Commands;

namespace VmcHmi.Application.Validators;

public class StartOperationCommandValidator : AbstractValidator<StartOperationCommand>
{
    public StartOperationCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
