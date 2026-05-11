using Viora.Domain.Abstractions;
using Viora.Domain.Vivi.Shared.Internals;

namespace Viora.Domain.Vivi.Shared;

public sealed class ChatSession : Entity
{
    /// <summary>
    /// Defines the user relation from the system that is declared to be chatting, nullable to allow unanimous chat sessions.
    /// </summary>
    public Guid? ChatterId { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    // TODO raise an event from the message creation to update the latest avtivity here
    public DateTime? LatestActivityUtc { get; private set; }
    public Persona Persona { get; private set; }
    public ModelUsed ModelUsed { get; private set; } = default!;

    private ChatSession(Guid id, Guid? chatterId, DateTime startedAtUtc, Persona persona, ModelUsed modelUsedName) : base(id)
    {
        ChatterId = chatterId;
        StartedAtUtc = startedAtUtc;
        Persona = persona;
        ModelUsed = modelUsedName;
    }

    private ChatSession() { }   // for EfCore

    public static Result<ChatSession> Create(Guid? chatterId, DateTime currentTimeUtc, Persona persona, string modelUsed)
    {
        return Result.Success(new ChatSession(Guid.NewGuid(), chatterId, currentTimeUtc, persona, modelUsed));
    }

    public Result UpdateLatestActivtyTime(DateTime currentDateTimeUtc)
    {
        if (LatestActivityUtc > currentDateTimeUtc)
            return Result.Failure(ViviErrors.ActivityTimeChatConflict);

        LatestActivityUtc = currentDateTimeUtc;

        return Result.Success();
    }
}