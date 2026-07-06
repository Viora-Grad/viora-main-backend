namespace Viora.Application.Marketing.GetDraftImage;

// Raw image bytes + content type, returned as a file by the controller.
public sealed record MarketingImageResponse(byte[] Content, string ContentType, string FileName);
