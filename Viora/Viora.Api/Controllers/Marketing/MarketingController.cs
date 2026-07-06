using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Marketing.ConnectMetaPage;
using Viora.Application.Marketing.DeleteMetaCredential;
using Viora.Application.Marketing.GetChat;
using Viora.Application.Marketing.GetDraftContent;
using Viora.Application.Marketing.GetDraftImage;
using Viora.Application.Marketing.GetMetaCredentialStatus;
using Viora.Application.Marketing.GetQuota;
using Viora.Application.Marketing.ListChats;
using Viora.Application.Marketing.PollContent;
using Viora.Application.Marketing.PublishPost;
using Viora.Application.Marketing.SaveMetaCredential;
using Viora.Application.Marketing.SendMessage;
using Viora.Application.Marketing.StartChat;

namespace Viora.Api.Controllers.Marketing;

[Route("api/marketing")]
[ApiController]
[Authorize]
public class MarketingController(ISender sender) : ControllerBase
{
    // Save/update the caller's organization Facebook Page access token + Page id.
    [HttpPost("meta-credentials")]
    public async Task<IActionResult> SaveMetaCredential(SaveMetaCredentialRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SaveMetaCredentialCommand(request.PageId, request.AccessToken), cancellationToken);
        return result.ToActionResult();
    }

    // Connect a Facebook Page via the OAuth flow: exchanges the short-lived user token (AuthCode) for a
    // long-lived one, resolves the Page's own token from /me/accounts by PageId, then stores it (encrypted).
    [HttpPost("meta-credentials/connect")]
    public async Task<IActionResult> ConnectMetaPage(ConnectMetaPageRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ConnectMetaPageCommand(request.AuthCode, request.PageId), cancellationToken);
        return result.ToActionResult();
    }

    // Report whether the caller's organization has a Facebook Page credential saved.
    [HttpGet("meta-credentials/status")]
    public async Task<IActionResult> GetMetaCredentialStatus(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMetaCredentialStatusQuery(), cancellationToken);
        return result.ToActionResult();
    }

    // Delete the caller's organization Facebook Page credential from the database.
    [HttpDelete("meta-credentials")]
    public async Task<IActionResult> DeleteMetaCredential(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteMetaCredentialCommand(), cancellationToken);
        return result.ToActionResult();
    }

    // Start a new chat session (== a new post draft), optionally with a first prompt.
    [HttpPost("chats")]
    public async Task<IActionResult> StartChat(StartChatRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new StartChatCommand(request.FirstMessage), cancellationToken);
        return result.ToActionResult();
    }

    // Send a message; runs intent detection + routing (Manus content or finalize).
    [HttpPost("chats/{chatId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid chatId, SendMarketingMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SendMarketingMessageCommand(chatId, request.Message), cancellationToken);
        return result.ToActionResult();
    }

    // Poll the in-flight Manus generation for a chat; returns the copy once ready (async two-step).
    [HttpPost("chats/{chatId:guid}/poll-content")]
    public async Task<IActionResult> PollContent(Guid chatId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new PollContentCommand(chatId), cancellationToken);
        return result.ToActionResult();
    }

    // Preview the generated draft image (proxied from Manus) before publishing. Returns the raw image bytes;
    // fetch it with the Authorization header (e.g. as a blob) rather than a bare <img src>.
    [HttpGet("chats/{chatId:guid}/image")]
    public async Task<IActionResult> GetDraftImage(Guid chatId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDraftImageQuery(chatId), cancellationToken);
        return result.IsSuccess
            ? File(result.Value.Content, result.Value.ContentType, result.Value.FileName)
            : result.ToActionResult();
    }

    // Preview the drafted post copy (proxied from the Manus attachment) before publishing.
    [HttpGet("chats/{chatId:guid}/content")]
    public async Task<IActionResult> GetDraftContent(Guid chatId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDraftContentQuery(chatId), cancellationToken);
        return result.ToActionResult();
    }

    // Get a chat session with message history and current status.
    [HttpGet("chats/{chatId:guid}")]
    public async Task<IActionResult> GetChat(Guid chatId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetChatQuery(chatId), cancellationToken);
        return result.ToActionResult();
    }

    // List the caller's organization chat sessions (title/status).
    [HttpGet("chats")]
    public async Task<IActionResult> ListChats(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListChatsQuery(), cancellationToken);
        return result.ToActionResult();
    }

    // Publish the archived post to Facebook.
    [HttpPost("chats/{chatId:guid}/publish")]
    public async Task<IActionResult> Publish(Guid chatId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new PublishPostCommand(chatId), cancellationToken);
        return result.ToActionResult();
    }

    // Get remaining/used marketing-post quota for the caller's organization.
    [HttpGet("quota")]
    public async Task<IActionResult> GetQuota(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMarketingQuotaQuery(), cancellationToken);
        return result.ToActionResult();
    }
}
