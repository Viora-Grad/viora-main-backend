using Viora.Domain.Abstractions;
using Viora.Domain.Vivi.ChatSessions.Internals;
using Viora.Domain.Vivi.Shared.Internals;

namespace Viora.Domain.Vivi.ChatSessions;

public sealed class ChatSession : Entity
{
    /// <summary>
    /// Defines the user relation from the system that is declared to be chatting, nullable to allow unanimous chat sessions.
    /// </summary>
    public Guid? ChatterId { get; private set; }
    /// <summary>
    /// defined as the first questionn asked for now, if it could be infeered from the question it would be better
    /// </summary>
    public Name Name { get; private set; } = default!;
    public DateTime StartedAtUtc { get; private set; }
    // TODO raise an event from the message creation to update the latest avtivity here
    public DateTime LatestActivityUtc { get; private set; }
    public Persona Persona { get; private set; }
    public ModelUsed ModelUsed { get; private set; } = default!;

    private ChatSession(Guid id, Guid? chatterId, Name name, DateTime startedAtUtc, Persona persona, ModelUsed modelUsedName) : base(id)
    {
        ChatterId = chatterId;
        Name = name;
        StartedAtUtc = startedAtUtc;
        Persona = persona;
        ModelUsed = modelUsedName;
    }

    private ChatSession() { }   // for EfCore

    public static Result<ChatSession> Create(Guid? chatterId, string name, DateTime currentTimeUtc, Persona persona, string modelUsed)
    {
        var result = new ChatSession(Guid.NewGuid(), chatterId, name, currentTimeUtc, persona, modelUsed)
        {
            LatestActivityUtc = currentTimeUtc
        };
        return Result.Success(result);
    }

    public Result UpdateLatestActivtyTime(DateTime currentDateTimeUtc)
    {
        if (LatestActivityUtc > currentDateTimeUtc)
            return Result.Failure(ChatSessionErrors.ActivityTimeChatConflict);

        LatestActivityUtc = currentDateTimeUtc;

        return Result.Success();
    }
}