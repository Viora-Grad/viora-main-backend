using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Services.AddDiscount;

public sealed record AddDiscountCommand(Guid ServiceId, int DiscountOutOf100, string Reason, TimeSpan Duration) : ICommand;