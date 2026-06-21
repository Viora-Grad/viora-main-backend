using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Customers.UpdateCustomerPicture;

public sealed record UpdateCustomerPictureCommand(
    Stream FileStream,
    string FileName,
    string MimeType,
    long SizeInBytes) : ICommand;
