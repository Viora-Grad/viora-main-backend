using Viora.Domain.Staffs;

namespace Viora.Infrastructure.Seeding.Data;

internal class StaffData
{
    public static IReadOnlyList<Staff> All { get; } = [
        new Staff(new Guid("e7b4a1c9-2d68-4f35-8b0e-6c9d1f2a7e54"),new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19")),
        new Staff(new Guid("f3c1a2d4-5e6b-4f7c-9a8d-1b2c3d4e5f6a"), new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19")),
        new Staff(new Guid("a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19")),
        new Staff(new Guid("b1c2d3e4-5f6a-7b8c-9d0e-1f2a3b4c5d6e"), new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19")),
        new Staff(new Guid("c1d2e3f4-5a6b-7c8d-9e0f-1a2b3c4d5e6f"), new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"))
        ];
}
