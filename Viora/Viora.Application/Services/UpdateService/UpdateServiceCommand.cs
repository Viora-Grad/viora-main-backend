using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Shared;

namespace Viora.Application.Services.UpdateService;

public sealed record UpdateServiceCommand(Guid ServiceId, string Name, string Description, string ServiceType, TimeSpan Duration, Money Cost) : ICommand;
