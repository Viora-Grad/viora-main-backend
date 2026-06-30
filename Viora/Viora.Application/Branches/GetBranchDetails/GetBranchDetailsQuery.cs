using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Branches.GetBranchDetails;

public record GetBranchDetailsQuery(Guid Id) : IQuery<BranchDetailsResponse>;