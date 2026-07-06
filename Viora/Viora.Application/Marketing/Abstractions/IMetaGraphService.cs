using Viora.Domain.Abstractions;

namespace Viora.Application.Marketing.Abstractions;

// Facebook Pages Graph API adapter. Posts are created LIVE (published=true) in a single call at publish time;
// there is no unpublished/archived state on Facebook (the draft lives locally until published).
public interface IMetaGraphService
{
    // Creates a published text/link post: POST /{page-id}/feed (message, link?, published=true). Returns the id.
    Task<Result<MetaPostResult>> CreatePostAsync(
        string pageId,
        string accessToken,
        MetaPostPayload payload,
        CancellationToken cancellationToken);

    // Creates a published photo post: POST /{page-id}/photos (caption, source=binary, published=true).
    // Returns the story post id ("{page-id}_{post-id}").
    Task<Result<MetaPostResult>> CreatePhotoPostAsync(
        string pageId,
        string accessToken,
        string caption,
        byte[] imageBytes,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);
}

// Content-only payload for POST /{page-id}/feed. access_token/page-id/published are injected by the adapter.
public sealed record MetaPostPayload(string Message, string? Link);

public sealed record MetaPostResult(string FacebookPostId);
