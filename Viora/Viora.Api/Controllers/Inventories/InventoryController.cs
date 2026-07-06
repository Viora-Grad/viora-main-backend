using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Viora.Api.Extensions;
using Viora.Application.Inventories.AddToInventory;
using Viora.Application.Inventories.GetBranchMovements;
using Viora.Application.Inventories.GetInventoryItemById;
using Viora.Application.Inventories.GetInventoryItemImage;
using Viora.Application.Inventories.GetInventoryItems;
using Viora.Application.Inventories.InventoryItemAction;
using Viora.Application.Inventories.UpdateInventoryItem;
using Viora.Domain.InventoryMovements.Internals;

namespace Viora.Api.Controllers.Inventories;

[Route("api")]
[ApiController]
public class InventoryController(ISender sender) : ControllerBase
{
    private Guid? UserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : null;

    [HttpGet]
    [Authorize(Policy = "inventory:read")]
    [Route("branch/{branchId}/inventories")]
    public async Task<IActionResult> GetItems(
        Guid branchId,
        [FromQuery] string? itemName,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetInventoryItemsQuery(branchId, itemName, page, pageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("inventories/{itemId:guid}")]
    [Authorize(Policy = "inventory:read")]
    public async Task<IActionResult> GetItemById(Guid itemId, CancellationToken cancellationToken)
    {
        var query = new GetInventoryItemByIdQuery(itemId);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [Authorize(Policy = "inventory:read")]
    [HttpGet("inventories/{itemId:guid}/image")]
    public async Task<IActionResult> GetItemImage(Guid itemId, CancellationToken cancellationToken)
    {
        var query = new GetInventoryItemImageQuery(itemId);
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        var file = result.Value;
        return File(file.Content, file.ContentType, file.FileName);
    }

    [Authorize(Policy = "inventory:read")]
    [HttpGet("branch/{branchId}/inventories/movements")]
    public async Task<IActionResult> GetBranchMovements(
        Guid branchId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBranchMovementsQuery(branchId, page, pageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "inventory:write")]
    [Route("inventories")]
    public async Task<IActionResult> AddItem(AddInventoryItemRequest request, CancellationToken cancellationToken)
    {
        var command = new AddToInventoryCommand(
            request.BranchId,
            UserId ?? Guid.Empty,
            request.PhotoId,
            request.Name,
            request.Notes,
            request.Quantity,
            request.MinimumThreshold);
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult(
            createdAtAction: nameof(GetItemById),
            routeValueFactory: val => new { itemId = val });
    }

    [HttpPut("inventories/{itemId:guid}")]
    [Authorize(Policy = "inventory:write")]
    public async Task<IActionResult> UpdateItem(Guid itemId, UpdateInventoryItemRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateInventoryItemCommand(
            itemId,
            request.PhotoId,
            request.Name,
            request.Notes,
            request.MinimumThreshold);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("inventories/{itemId:guid}/restock")]
    [Authorize(Policy = "inventory:write")]
    public async Task<IActionResult> Restock(Guid itemId, InventoryItemActionRequest request, CancellationToken cancellationToken)
    {
        var command = new InventoryItemActionCommand(
            itemId,
            UserId ?? Guid.Empty,
            request.Quantity,
            request.Notes,
            InventoryMovementType.Restock);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("inventories/{itemId:guid}/consume")]
    [Authorize(Policy = "inventory:write")]
    public async Task<IActionResult> Consume(Guid itemId, InventoryItemActionRequest request, CancellationToken cancellationToken)
    {
        var command = new InventoryItemActionCommand(
            itemId,
            UserId ?? Guid.Empty,
            request.Quantity,
            request.Notes,
            InventoryMovementType.Consume);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

}
