using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Shared;

namespace Viora.Application.Services.AddService;

public sealed record AddServiceCommand(Guid BranchId, string Name, string Description, string ServiceType, TimeSpan Duration, Money Cost) : ICommand<Guid>;
