using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Branches.UnlinkImageFromBranch;

public sealed record UnlinkImageFromBranchCommand(Guid BranchId, Guid ImageId) : ICommand;