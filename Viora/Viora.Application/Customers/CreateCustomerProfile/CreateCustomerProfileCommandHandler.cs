using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Users.Customers;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;
using Email = Viora.Domain.Users.Internal.Email;

namespace Viora.Application.Customers.CreateCustomerProfile;

internal class CreateCustomerProfileCommandHandler(
    IUserContext userContext,
    IUserRepository userRepository,
    ICustomerRepository customerRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateCustomerProfileCommand, CreateCustomerProfileResponse>
{
    public async Task<Result<CreateCustomerProfileResponse>> Handle(CreateCustomerProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var user = await userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new NotFoundException("User not found.");
        var HasCustomerProfile = await customerRepository.GetByIdAsync(userId, cancellationToken);
        if (HasCustomerProfile is not null)
        {
            throw new ConflictException("Customer profile already exists for this user.");
        }
        var userName = request.UserName is not null ? new UserName(request.UserName) : null;
        var customer = Customer.Create(
            userId,
            userName,
            user.PersonalInfo,
            dateTimeProvider.UtcNow,
            request.PhoneNumbers.Select(p => new PhoneNumber(p)).ToArray(),
            request.Emails.Select(e => new Email(e)).ToArray()
            );
        customerRepository.Add(customer);

        var role = await userRepository.FindRoleAsync(Role.Customer.Id, cancellationToken) ?? throw new NotFoundException("Customer role not found.");
        user.BecomeCustomer(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateCustomerProfileResponse(
            customer.Id,
            customer.UserName?.Value,
            customer.PersonalInfo.FirstName,
            customer.PersonalInfo.LastName,
            customer.PersonalInfo.DateOfBirth,
            customer.PersonalInfo.Gender.ToString(),
            customer.PhoneNumbers.Select(p => p.Value).ToList(),
            customer.Emails.Select(e => e.Value).ToList(),
            customer.JoinedAt
            );

        return Result.Success(response);
    }
}
