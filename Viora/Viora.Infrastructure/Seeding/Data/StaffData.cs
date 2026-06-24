using Viora.Domain.Staffs;

namespace Viora.Infrastructure.Seeding.Data;

internal class StaffData
{
    public static IReadOnlyList<Staff> All { get; } = [
        new Staff(new Guid("e7b4a1c9-2d68-4f35-8b0e-6c9d1f2a7e54"),new Guid("E0763867-4F60-43A7-A7E0-444528C3802B"))
        ];
}
