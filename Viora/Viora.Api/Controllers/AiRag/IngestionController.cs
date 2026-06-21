using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Application.AiRag.Ingestion;

namespace Viora.Api.Controllers.AiRag;

[ApiController]
[Route("api/ai/ingestion")]
// [Authorize(Policy = "AdminOnly")] // CAN SOMEONE PLEASE SEE THIS POLICY IF IT IS RIGHT OR NOT :)
public sealed class IngestionController : ControllerBase
{
    private readonly IngestKnowledgeCommand _ingest;
    private readonly IngestSpecialtyCommand _ingestSpecialty;
    private readonly IConfiguration _config;
    private readonly ILogger<IngestionController> _logger;

    public IngestionController(
        IngestKnowledgeCommand ingest,
        IngestSpecialtyCommand ingestSpecialty,
        IConfiguration config,
        ILogger<IngestionController> logger)
    {
        _ingest = ingest;
        _ingestSpecialty = ingestSpecialty;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Reads the knowledge-base file from the configured path and ingests it into Qdrant.
    /// Idempotent — safe to re-run after updating the file.
    /// </summary>
    [HttpPost("knowledge")]
    public async Task<IActionResult> IngestKnowledgeFromFile(CancellationToken ct)
    {
        try
        {
            var path = _config["AiRag:KnowledgeBase:FilePath"];
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(new { Error = "AiRag:KnowledgeBase:FilePath not configured." });

            if (!System.IO.File.Exists(path))
                return NotFound(new { Error = $"Knowledge file not found at: {path}" });

            var content = await System.IO.File.ReadAllTextAsync(path, ct);
            await _ingest.ExecuteAsync(content, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Knowledge ingestion from file failed");
            return StatusCode(500, new { Error = $"Ingestion failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Reads the specialty JSON file from the configured path and ingests it into Qdrant.
    /// Idempotent — safe to re-run after updating the file.
    /// </summary>
    [HttpPost("specialty")]
    public async Task<IActionResult> IngestSpecialtyFromFile(CancellationToken ct)
    {
        try
        {
            var path = _config["AiRag:SpecialtyBase:FilePath"];
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(new { Error = "AiRag:SpecialtyBase:FilePath not configured." });
            if (!System.IO.File.Exists(path))
                return NotFound(new { Error = $"Specialty file not found at: {path}" });

            await using var stream = System.IO.File.OpenRead(path);
            await _ingestSpecialty.ExecuteAsync(SpecialtyInquiryParser.ParseAsync(stream, ct), ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Specialty ingestion from file failed");
            return StatusCode(500, new { Error = $"Ingestion failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Accepts specialty JSON content in the request body directly.
    /// </summary>
    [HttpPost("specialty/raw")]
    public async Task<IActionResult> IngestSpecialtyFromBody([FromBody] JsonElement json, CancellationToken ct)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(json.GetRawText());
            using var stream = new MemoryStream(bytes);
            await _ingestSpecialty.ExecuteAsync(SpecialtyInquiryParser.ParseAsync(stream, ct), ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Specialty ingestion from body failed");
            return StatusCode(500, new { Error = $"Ingestion failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Accepts markdown content in the request body directly.
    /// </summary>
    [HttpPost("knowledge/raw")]
    public async Task<IActionResult> IngestFromBody([FromBody] IngestRequest request, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.MarkdownContent))
                return BadRequest(new { Error = "MarkdownContent is required." });

            await _ingest.ExecuteAsync(request.MarkdownContent, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Knowledge ingestion from body failed");
            return StatusCode(500, new { Error = $"Ingestion failed: {ex.Message}" });
        }
    }

}

public sealed record IngestRequest(string MarkdownContent);