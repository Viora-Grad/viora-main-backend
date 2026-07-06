using FluentValidation;

namespace Viora.Application.Marketing.SendMessage;

internal sealed class SendMarketingMessageCommandValidator : AbstractValidator<SendMarketingMessageCommand>
{
    public SendMarketingMessageCommandValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty().WithMessage("Chat id is required.");
        RuleFor(x => x.Message).NotEmpty().WithMessage("Message is required.").MaximumLength(4000);
    }
}
