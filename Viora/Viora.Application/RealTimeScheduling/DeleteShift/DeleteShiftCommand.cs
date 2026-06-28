using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.RealTimeScheduling.DeleteShift;

public record DeleteShiftCommand(Guid Id) : ICommand;
