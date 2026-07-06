using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Viora.Application.Marketing.Abstractions;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;

namespace Viora.Infrastructure.Marketing;

// Manus marketing-content adapter (typed HttpClient). Manus is agentic/async: create a task, poll its
// status, and read the agent's output once it stops. Contract per https://open.manus.im/docs (v2).
//
//   Create:   POST {BaseUrl}task.create          header x-manus-api-key
//       body:     { "message": { "content": [ { "type": "text", "text": "..." } ] } }
//       response: { "ok": true, "task_id": "...", "task_url": "...", ... }
//
//   Status:   GET  {BaseUrl}task.detail?task_id=...
//       response: { "task": { "status": "running|stopped|waiting|error", ... } }
//       "stopped" = finished, "error" = failed, "running"/"waiting" = still in progress.
//
//   Output:   GET  {BaseUrl}task.listMessages?task_id=...&order=desc
//       response: { "messages": [ { "type": "assistant_message", "assistant_message": { "content": "..." } }, ... ] }
//       The copy is the latest assistant_message.content.
//
// BaseUrl is expected to end with the version segment (e.g. https://api.manus.ai/v2/); the x-manus-api-key
// header is attached at DI registration.
internal sealed class ManusClient : IManusClient
{
    private const string StatusStopped = "stopped";
    private const string StatusError = "error";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ManusClient> _logger;

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public ManusClient(HttpClient httpClient, ILogger<ManusClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<ManusTaskRef>> CreateTaskAsync(string content, CancellationToken cancellationToken)
    {
        var body = new { message = new { content = new[] { new { type = "text", text = content } } } };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync("task.create", body, WriteOptions, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Manus task.create failed ({Status}): {Body}", (int)response.StatusCode, raw);
                return Result.Failure<ManusTaskRef>(MarketingErrors.ManusFailed);
            }

            using var doc = JsonDocument.Parse(raw);
            var taskId = GetString(doc.RootElement, "task_id");
            if (string.IsNullOrWhiteSpace(taskId))
            {
                _logger.LogError("Manus task.create response missing task_id: {Body}", raw);
                return Result.Failure<ManusTaskRef>(MarketingErrors.ManusFailed);
            }

            return Result.Success(new ManusTaskRef(taskId, GetString(doc.RootElement, "task_url")));
        }
        catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException or JsonException)
        {
            _logger.LogError(ex, "Manus task.create call failed.");
            return Result.Failure<ManusTaskRef>(MarketingErrors.ManusFailed);
        }
    }

    public async Task<Result<ManusTaskResult>> GetTaskResultAsync(string taskId, CancellationToken cancellationToken)
    {
        var escaped = Uri.EscapeDataString(taskId);

        try
        {
            // 1. Authoritative status from task.detail.
            using var detailResponse = await _httpClient.GetAsync($"task.detail?task_id={escaped}", cancellationToken);
            var detailRaw = await detailResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!detailResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Manus task.detail failed ({Status}) for {TaskId}: {Body}", (int)detailResponse.StatusCode, taskId, detailRaw);
                return Result.Failure<ManusTaskResult>(MarketingErrors.ManusFailed);
            }

            string? status;
            using (var detailDoc = JsonDocument.Parse(detailRaw))
            {
                status = detailDoc.RootElement.TryGetProperty("task", out var task) && task.TryGetProperty("status", out var s)
                    ? s.GetString()
                    : null;
            }

            if (string.Equals(status, StatusError, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Manus task {TaskId} ended in error.", taskId);
                return Result.Failure<ManusTaskResult>(MarketingErrors.ManusFailed);
            }

            // running / waiting / anything not-yet-terminal => still generating.
            if (!string.Equals(status, StatusStopped, StringComparison.OrdinalIgnoreCase))
                return Result.Success(new ManusTaskResult(Completed: false, Content: null));

            // 2. Stopped => pull the latest assistant output from task.listMessages.
            using var messagesResponse = await _httpClient.GetAsync($"task.listMessages?task_id={escaped}&order=desc&limit=50", cancellationToken);
            var messagesRaw = await messagesResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!messagesResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Manus task.listMessages failed ({Status}) for {TaskId}: {Body}", (int)messagesResponse.StatusCode, taskId, messagesRaw);
                return Result.Failure<ManusTaskResult>(MarketingErrors.ManusFailed);
            }

            using var messagesDoc = JsonDocument.Parse(messagesRaw);
            var content = ExtractLatestAssistantContent(messagesDoc.RootElement);
            var imageUrl = ExtractLatestAssistantImageUrl(messagesDoc.RootElement);
            var contentUrl = ExtractLatestAssistantContentUrl(messagesDoc.RootElement);
            return Result.Success(new ManusTaskResult(Completed: true, Content: content, ImageUrl: imageUrl, ContentUrl: contentUrl));
        }
        catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException or JsonException)
        {
            _logger.LogError(ex, "Manus task result poll failed for {TaskId}.", taskId);
            return Result.Failure<ManusTaskResult>(MarketingErrors.ManusFailed);
        }
    }

    // Downloads a Manus attachment. The client's x-manus-api-key header rides along even for the absolute
    // files URL, so private attachments are fetchable here (Facebook can't fetch them directly).
    public async Task<Result<ManusImage>> DownloadImageAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Manus image download failed ({Status}) for {Url}.", (int)response.StatusCode, url);
                return Result.Failure<ManusImage>(MarketingErrors.ManusFailed);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/png";
            return Result.Success(new ManusImage(bytes, contentType, DeriveFileName(url, contentType)));
        }
        catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException)
        {
            _logger.LogError(ex, "Manus image download errored for {Url}.", url);
            return Result.Failure<ManusImage>(MarketingErrors.ManusFailed);
        }
    }

    // Downloads a Manus text attachment (the post copy) and decodes it as a string. The client's
    // x-manus-api-key header rides along even for the absolute files URL, so private attachments are
    // fetchable here (the raw URL is not publicly readable).
    public async Task<Result<ManusText>> DownloadTextAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Manus text download failed ({Status}) for {Url}.", (int)response.StatusCode, url);
                return Result.Failure<ManusText>(MarketingErrors.ManusFailed);
            }

            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "text/plain";
            return Result.Success(new ManusText(text, contentType));
        }
        catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException)
        {
            _logger.LogError(ex, "Manus text download errored for {Url}.", url);
            return Result.Failure<ManusText>(MarketingErrors.ManusFailed);
        }
    }

    // messages are returned newest-first (order=desc); the first assistant_message carries the final copy.
    private static string? ExtractLatestAssistantContent(JsonElement root)
    {
        if (!root.TryGetProperty("messages", out var messages) || messages.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var message in messages.EnumerateArray())
        {
            if (message.TryGetProperty("type", out var type)
                && type.GetString() == "assistant_message"
                && message.TryGetProperty("assistant_message", out var assistant)
                && assistant.TryGetProperty("content", out var content)
                && content.ValueKind == JsonValueKind.String)
            {
                var text = content.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }
        }

        return null;
    }

    // Latest assistant image attachment url. An attachment is an image when type=="image" (or content_type
    // starts with "image/"). Newest-first order => first match is the most recent generated image.
    private static string? ExtractLatestAssistantImageUrl(JsonElement root)
    {
        if (!root.TryGetProperty("messages", out var messages) || messages.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var message in messages.EnumerateArray())
        {
            if (!message.TryGetProperty("type", out var type) || type.GetString() != "assistant_message")
                continue;
            if (!message.TryGetProperty("assistant_message", out var assistant)
                || !assistant.TryGetProperty("attachments", out var attachments)
                || attachments.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var attachment in attachments.EnumerateArray())
            {
                var attachmentType = GetString(attachment, "type");
                var contentType = GetString(attachment, "content_type");
                var isImage = string.Equals(attachmentType, "image", StringComparison.OrdinalIgnoreCase)
                    || (contentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ?? false);

                var url = GetString(attachment, "url");
                if (isImage && !string.IsNullOrWhiteSpace(url))
                    return url;
            }
        }

        return null;
    }

    // Latest assistant non-image attachment url — the post copy, which Manus attaches as a document
    // (e.g. text/markdown) rather than inlining in content. Newest-first order => first match is most recent.
    private static string? ExtractLatestAssistantContentUrl(JsonElement root)
    {
        if (!root.TryGetProperty("messages", out var messages) || messages.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var message in messages.EnumerateArray())
        {
            if (!message.TryGetProperty("type", out var type) || type.GetString() != "assistant_message")
                continue;
            if (!message.TryGetProperty("assistant_message", out var assistant)
                || !assistant.TryGetProperty("attachments", out var attachments)
                || attachments.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var attachment in attachments.EnumerateArray())
            {
                var attachmentType = GetString(attachment, "type");
                var contentType = GetString(attachment, "content_type");
                var isImage = string.Equals(attachmentType, "image", StringComparison.OrdinalIgnoreCase)
                    || (contentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ?? false);

                var url = GetString(attachment, "url");
                if (!isImage && !string.IsNullOrWhiteSpace(url))
                    return url;
            }
        }

        return null;
    }

    // A filename with a sensible extension keeps Facebook's photo upload happy.
    private static string DeriveFileName(string url, string contentType)
    {
        var name = Uri.TryCreate(url, UriKind.Absolute, out var uri)
            ? Path.GetFileName(uri.LocalPath)
            : null;

        if (!string.IsNullOrWhiteSpace(name) && Path.HasExtension(name))
            return name!;

        var ext = contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => ".png",
        };
        return $"manus-image{ext}";
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
