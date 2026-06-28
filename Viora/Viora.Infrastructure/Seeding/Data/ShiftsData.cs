
using Viora.Domain.RealTimeScheduling;

namespace Viora.Infrastructure.Seeding.Data;

internal static class ShiftsData
{
    public static IReadOnlyCollection<Shift> All =
    [
        Shift.Create(new Guid("AEC9F604-DB9D-4273-95A5-88DED795AD5D"), new TimeOnly(12 , 0), new TimeOnly(23, 59),new Guid("C1D2E3F4-5A6B-7C8D-9E0F-1A2B3C4D5E6F")),
        Shift.Create(new Guid("02967DEF-7D97-42EC-ADC6-89AEA592A204"), new TimeOnly(12 , 0), new TimeOnly(23, 59),new Guid("C1D2E3F4-5A6B-7C8D-9E0F-1A2B3C4D5E6F")),
        Shift.Create(new Guid("A2202230-5353-4B6A-9D54-2754ABEF6867"), new TimeOnly(12 , 0), new TimeOnly(23, 59),new Guid("A1B2C3D4-5E6F-7A8B-9C0D-1E2F3A4B5C6D")),
        Shift.Create(new Guid("BA7AEF31-2F7C-4A56-BF73-9DC4C195D055"), new TimeOnly(12 , 0), new TimeOnly(23, 59),new Guid("B1C2D3E4-5F6A-7B8C-9D0E-1F2A3B4C5D6E")),
        Shift.Create(new Guid("AE77FCBF-048F-47CA-A358-88F6BD0B75BC"), new TimeOnly(12 , 0), new TimeOnly(23, 59),new Guid("E7B4A1C9-2D68-4F35-8B0E-6C9D1F2A7E54")),


    ];
}
