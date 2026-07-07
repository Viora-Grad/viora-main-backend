using Viora.Domain.Abstractions;

namespace Viora.Application.Marketing.Abstractions;

// Wraps the Manus marketing-content API. Manus runs generation as an ASYNC task: CreateTaskAsync starts a
// task and returns its id/url immediately; GetTaskResultAsync is polled until the task completes. When the
// agent produced an image, GetTaskResultAsync surfaces its (API-key-gated) URL, which DownloadImageAsync
// fetches as bytes for re-upload to Facebook. The finished post copy is delivered the same way — as a
// (non-image) attachment whose API-key-gated URL DownloadTextAsync fetches as text for previewing.
public interface IManusClient
{
    Task<Result<ManusTaskRef>> CreateTaskAsync(string content, CancellationToken cancellationToken);

    Task<Result<ManusTaskResult>> GetTaskResultAsync(string taskId, CancellationToken cancellationToken);

    Task<Result<ManusImage>> DownloadImageAsync(string url, CancellationToken cancellationToken);

    Task<Result<ManusText>> DownloadTextAsync(string url, CancellationToken cancellationToken);
}

// task.create response: the task handle to poll on.
public sealed record ManusTaskRef(string TaskId, string? TaskUrl);

// task.get/listMessages, normalized: Completed=false means still running; when true, Content is the copy,
// ImageUrl (if any) is the Manus URL of a generated image attachment, and ContentUrl (if any) is the Manus
// URL of the post-copy attachment (Manus delivers the full copy as an attached document).
public sealed record ManusTaskResult(bool Completed, string? Content, string? ImageUrl = null, string? ContentUrl = null);

// A downloaded Manus image, ready to upload to Facebook as a photo.
public sealed record ManusImage(byte[] Bytes, string ContentType, string FileName);

// A downloaded Manus text attachment (the post copy), decoded for previewing.
public sealed record ManusText(string Text, string ContentType);
