namespace Viora.Application.Staffs.SearchStaff;

public class SearchStaffResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }

    public string Gender { get; set; }
    public DateOnly DateOfBirth { get; set; }



}