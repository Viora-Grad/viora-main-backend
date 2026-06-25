using Viora.Domain.Staffs;

namespace Viora.Infrastructure.Seeding.Data;

internal class StaffData
{
    public static IReadOnlyList<Staff> All { get; } = [
        new Staff(new Guid("e7b4a1c9-2d68-4f35-8b0e-6c9d1f2a7e54"),new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"))
        ];
}
