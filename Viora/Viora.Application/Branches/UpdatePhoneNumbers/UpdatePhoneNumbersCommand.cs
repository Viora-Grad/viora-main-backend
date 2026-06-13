using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Branches.UpdatePhoneNumbers;

public sealed record UpdatePhoneNumbersCommand(Guid BranchId, ICollection<string> PhoneNumbers) : ICommand;
