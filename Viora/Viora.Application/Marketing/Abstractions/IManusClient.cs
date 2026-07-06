using Viora.Domain.Abstractions;

namespace Viora.Application.Marketing.Abstractions;

// Wraps the Manus marketing-content API. Manus runs generation as an ASYNC task: CreateTaskAsync starts a
// task and returns its id/url immediately; GetTaskResultAsync is polled until the task completes. When the
// agent produced an image, GetTaskResultAsync surfaces its (API-key-gated) URL, which DownloadImageAsync
// fetches as bytes for re-upload to Facebook.
public interface IManusClient
{
    Task<Result<ManusTaskRef>> CreateTaskAsync(string content, CancellationToken cancellationToken);

    Task<Result<ManusTaskResult>> GetTaskResultAsync(string taskId, CancellationToken cancellationToken);

    Task<Result<ManusImage>> DownloadImageAsync(string url, CancellationToken cancellationToken);
}

// task.create response: the task handle to poll on.
public sealed record ManusTaskRef(string TaskId, string? TaskUrl);

// task.get/listMessages, normalized: Completed=false means still running; when true, Content is the copy and
// ImageUrl (if any) is the Manus URL of a generated image attachment.
public sealed record ManusTaskResult(bool Completed, string? Content, string? ImageUrl = null);

// A downloaded Manus image, ready to upload to Facebook as a photo.
public sealed record ManusImage(byte[] Bytes, string ContentType, string FileName);
