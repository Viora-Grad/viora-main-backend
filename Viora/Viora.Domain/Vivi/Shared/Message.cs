using Viora.Domain.Abstractions;
using Viora.Domain.Vivi.Shared.Internals;

namespace Viora.Domain.Vivi.Shared;

public sealed class Message : Entity
{
    public Guid SessionId { get; private set; }
    public MessageContent Content { get; private set; } = default!;
    public Role SenderRole { get; private set; }
    public DateTime SentAtUtc { get; private set; }
    public UsageDetails UsageDetails { get; private set; } = default!;

    private Message(Guid id, Guid sessionId, MessageContent content, Role senderRole, DateTime sentAtUtc, UsageDetails usageDetails) : base(id)
    {
        SessionId = sessionId;
        Content = content;
        SenderRole = senderRole;
        SentAtUtc = sentAtUtc;
        UsageDetails = usageDetails;
    }

    private Message() { }   // for EfCore

    public static Result<Message> Create(
        Guid sessionId,
        string content,
        Role senderRole,
        DateTime currentDateTime,
        int? inputTokens = null,
        int? outputTokens = null,
        int? latency = null)
    {
        if (senderRole == Role.Assistant && string.IsNullOrEmpty(content))
            return Result.Failure<Message>(ViviErrors.AgentFailedToLoadContent);

        return Result.Success(new Message(Guid.NewGuid(), sessionId, content, senderRole, currentDateTime, new(inputTokens, outputTokens, latency)));
    }
}
