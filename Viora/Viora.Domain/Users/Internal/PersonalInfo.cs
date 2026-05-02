namespace Viora.Domain.Users.Internal;

public sealed record PersonalInfo(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    Gender Gender)
{
    public int GetAge(DateOnly today)
    {
        int age = today.Year - DateOfBirth.Year;

        if (today < DateOfBirth.AddYears(age))
        {
            age--;
        }

        return age;
    }

}
