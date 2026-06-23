using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Branches.LinkImageToBranch;

public sealed record LinkImageToBranchCommand(Guid BranchId, Guid MediaId) : ICommand;