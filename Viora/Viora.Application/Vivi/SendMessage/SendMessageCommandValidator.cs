using FluentValidation;

namespace Viora.Application.Vivi.SendMessage;

internal class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(s => s.Message)
            .NotEmpty()
            .MaximumLength(500);
    }
}
