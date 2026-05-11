using Viora.Domain.Abstractions;
using Viora.Domain.Vivi.Shared.Internals;

namespace Viora.Domain.Vivi.Shared;

// TODO seed those broddy

/// <summary>
/// Internally set from the seeder without exposing the resource to external interface to create a question for now
/// </summary>
public sealed class SuggestedQuestion : Entity
{
    public Persona Persona { get; private set; }
    public Body Body { get; private set; } = default!;
    public ChatDepth ChatDepth { get; private set; } = default!;
    public QuestionCategory Category { get; private set; }

    private SuggestedQuestion() { }     // for EfCore
}
