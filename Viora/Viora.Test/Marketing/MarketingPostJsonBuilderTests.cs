using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Viora.Application.Marketing.Services;
using Viora.Domain.Marketing;

namespace Viora.Test.Marketing;

// Verifies the Groq JSON builder: valid parse, retry-on-malformed, terminal failure, and that only content
// fields are surfaced (a stray access_token in the model output is never carried through).
[TestClass]
public sealed class MarketingPostJsonBuilderTests
{
    private readonly Mock<IChatCompletionService> _chat = new();

    private MarketingPostJsonBuilder CreateBuilder() =>
        new(_chat.Object, NullLogger<MarketingPostJsonBuilder>.Instance);

    private static IReadOnlyList<ChatMessageContent> Reply(string content) =>
        new List<ChatMessageContent> { new(AuthorRole.Assistant, content) };

    private void SetupReply(string content) =>
        _chat.Setup(c => c.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings?>(),
                It.IsAny<Kernel?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Reply(content));

    [TestMethod]
    public async Task Valid_json_produces_post()
    {
        SetupReply("""{ "title": "Summer Sale", "message": "Big summer sale!", "link": "https://shop.example.com" }""");

        var result = await CreateBuilder().BuildAsync("idea", "context", CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Summer Sale", result.Value.Title);
        Assert.AreEqual("Big summer sale!", result.Value.Message);
        Assert.AreEqual("https://shop.example.com", result.Value.Link);
    }

    [TestMethod]
    public async Task Fenced_json_with_extra_token_field_is_parsed_and_token_not_surfaced()
    {
        // Model wraps in fences and (wrongly) includes an access_token; the builder must strip fences and
        // only ever expose title/message/link — the token has nowhere to go on GeneratedPost.
        SetupReply("""
            ```json
            { "title": "T", "message": "Hello", "link": null, "access_token": "SECRET-LEAK" }
            ```
            """);

        var result = await CreateBuilder().BuildAsync("idea", "context", CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Hello", result.Value.Message);
        Assert.IsNull(result.Value.Link);
        // GeneratedPost has no token member; assert none of its values echoed the leaked token.
        Assert.IsFalse((result.Value.Title ?? "").Contains("SECRET-LEAK"));
        Assert.IsFalse(result.Value.Message.Contains("SECRET-LEAK"));
    }

    [TestMethod]
    public async Task Malformed_then_valid_succeeds_via_retry()
    {
        _chat.SetupSequence(c => c.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings?>(),
                It.IsAny<Kernel?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Reply("this is not json at all"))
            .ReturnsAsync(Reply("""{ "title": "T", "message": "Recovered", "link": null }"""));

        var result = await CreateBuilder().BuildAsync("idea", "context", CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Recovered", result.Value.Message);
    }

    [TestMethod]
    public async Task Always_malformed_fails()
    {
        SetupReply("still not json");

        var result = await CreateBuilder().BuildAsync("idea", "context", CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(MarketingErrors.ContentGenerationFailed, result.Error);
    }
}
