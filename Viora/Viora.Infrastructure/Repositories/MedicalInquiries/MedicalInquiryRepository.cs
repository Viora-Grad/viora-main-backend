using System.Text.Json;
using Viora.Domain.MedicalInquiries;

namespace Viora.Infrastructure.Repositories.MedicalInquiries;

public class MedicalInquiryRepository
{
    string filePath = "./Data/MedicalInquiries.json";
    
    public IAsyncEnumerable<MedicalInquiry> GetAll()
    {
        using FileStream openStream = File.OpenRead(filePath);

        IAsyncEnumerable<MedicalInquiryJson> inquiryStream = JsonSerializer.DeserializeAsyncEnumerable<MedicalInquiryJson>(openStream);

        return inquiryStream.Select(item => new MedicalInquiry()
        {
            Id = Guid.NewGuid().ToString(),
            Question = item.Question,
            Specialty = item.Category,
        });
    }
}