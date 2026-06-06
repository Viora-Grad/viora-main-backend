namespace Viora.Domain.Organizations.LegalPapers;

public interface ILegalPapersSettings
{
    public TimeSpan LegalPaperExpiry { get; }
}
