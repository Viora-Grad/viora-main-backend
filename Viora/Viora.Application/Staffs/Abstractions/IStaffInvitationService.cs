namespace Viora.Application.Staffs.Abstractions;

public interface IStaffInvitationService
{
    string GenerateInvitationToken();
    string HashInvitationToken(string token);
    DateTime GetExpiryDate();
}
