using Viora.Domain.Services;
using Viora.Domain.Shared;

namespace Viora.Infrastructure.Seeding.Data;

internal class ServiceData(IServiceSettings ST)
{

    public IEnumerable<Service> All =
    [
        Service.Create(new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"), "Haircut", "A basic haircut service.", (int)TimeSpan.FromMinutes(30).TotalMinutes,ServiceType.Cardiology ,new Money(20.00m , Currency.Egp), ST).Value,
        Service.Create(new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"), "Hair Coloring", "A professional hair coloring service.", (int)TimeSpan.FromMinutes(90).TotalMinutes, ServiceType.Dermatology, new Money(100.00m, Currency.Egp), ST).Value,
        Service.Create(new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"), "Manicure", "A complete manicure service.", (int)TimeSpan.FromMinutes(40).TotalMinutes, ServiceType.Otolaryngology, new Money(30.00m, Currency.Egp), ST).Value,
        Service.Create(new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"), "Pedicure", "A complete pedicure service.", (int)TimeSpan.FromMinutes(60).TotalMinutes, ServiceType.Endocrinology, new Money(40.00m, Currency.Egp), ST).Value,
        Service.Create(new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"), "Facial", "A complete facial service.", (int)TimeSpan.FromMinutes(60).TotalMinutes, ServiceType.Endocrinology, new Money(50.00m, Currency.Egp), ST).Value,
        Service.Create(new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"), "Massage", "A relaxing massage service.", (int)TimeSpan.FromMinutes(60).TotalMinutes, ServiceType.Dermatology, new Money(70.00m, Currency.Egp), ST).Value,
        Service.Create(new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"), "Makeup Application", "A professional makeup application service.", (int)TimeSpan.FromMinutes(60).TotalMinutes, ServiceType.Cardiology, new Money(80.00m, Currency.Egp), ST).Value,
        Service.Create(new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"), "Waxing", "A complete waxing service.", (int)TimeSpan.FromMinutes(30).TotalMinutes, ServiceType.Cardiology, new Money(25.00m, Currency.Egp), ST).Value
    ];
}
