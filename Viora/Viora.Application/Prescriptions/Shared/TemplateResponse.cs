using Viora.Application.Abstractions.Media;

namespace Viora.Application.Prescriptions.Shared;

public class TemplateResponse
{

    public Guid Id { get; set; }
    public Guid OrganizaionId { get; set; }
    public MediaResponse? Media { get; set; }
    public string Name { get; set; }
    public double TopMargin { get; private set; }
    public double RightMargin { get; private set; }
    public double LeftMargin { get; private set; }
    public double BottomMargin { get; private set; }
    public TemplateResponse(Guid id, Guid organizaionId, MediaResponse? media, string name, double topMargin, double rightMargin, double leftMargin, double bottomMargin)
    {
        Id = id;
        OrganizaionId = organizaionId;
        Media = media;
        Name = name;
        TopMargin = topMargin;
        RightMargin = rightMargin;
        LeftMargin = leftMargin;
        BottomMargin = bottomMargin;
    }

}
