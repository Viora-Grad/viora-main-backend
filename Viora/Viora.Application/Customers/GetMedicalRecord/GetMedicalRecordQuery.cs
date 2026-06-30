using Viora.Application.Abstractions.Messaging;
using Viora.Domain.MedicalRecords;

namespace Viora.Application.Customers.GetMedicalRecord;

public sealed record GetMedicalRecordQuery() : IQuery<MedicalRecord>;
