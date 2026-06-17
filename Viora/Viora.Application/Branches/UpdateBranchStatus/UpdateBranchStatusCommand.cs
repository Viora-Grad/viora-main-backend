using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Branches.Internals;

namespace Viora.Application.Branches.UpdateBranchStatus;

public sealed record UpdateBranchStatusCommand(Guid Id, BranchStatus Status) : ICommand;
