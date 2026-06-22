using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Users.Customers;
namespace Viora.Application.Customers.UpdateCustomerPicture;

internal class UpdateCustomerPictureCommandHandler(
    IMediaRepository mediaRepository,
    IStorageSettings storageSettings,
    ICustomerRepository customerRepository,
    IUserContext userContext,
    IStorageService storageService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) :
    ICommandHandler<UpdateCustomerPictureCommand>
{
    public async Task<Result> Handle(UpdateCustomerPictureCommand request, CancellationToken cancellationToken)
    {
        var customerId = userContext.UserId;
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken) ??
            throw new NotFoundException("Customer profile not found");

        if (customer.ProfilePicId is not null)
        {
            var existingMedia = await mediaRepository.GetByIdAsync(customer.ProfilePicId.Value, cancellationToken);
            if (existingMedia is not null)
            {
                storageService.DeleteFile(existingMedia.Key);
            }
        }
        var extension = Path.GetExtension(request.FileName);
        var storageKey = $"customer-profile-pictures/{customerId}/{Guid.NewGuid()}{extension}";

        await storageService.SaveFileAsync(request.FileStream, storageKey, cancellationToken);

        var mediaResult = MediaFile.Create(
            request.FileName,
            request.SizeInBytes,
            storageKey,
            request.MimeType,
            dateTimeProvider.UtcNow,
            storageSettings.MaxFileSizeBytes,
            null
            );

        if (mediaResult.IsFailure)
            return Result.Failure(mediaResult.Error);

        var media = mediaResult.Value;
        mediaRepository.Add(media);

        var updateResult = customer.UpdateProfilePicture(media.Id);
        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

