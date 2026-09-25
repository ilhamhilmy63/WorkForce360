using System.Data;
using Microsoft.EntityFrameworkCore;
using WorkForce360.Api.Data;
using WorkForce360.Api.Models;

namespace WorkForce360.Api.Services;

public sealed class ResourceHoldService(TripDbContext db)
{
    public async Task<ResourceHoldResult> CreateAsync(
        CreateResourceHold request,
        CancellationToken cancellationToken)
    {
        var type = request.ResourceType.Trim().ToLowerInvariant();
        var errors = ResourceAvailabilityRules.ValidateHold(
            type, request.FromDate, request.ToDate, request.Quantity);
        if (errors.Count > 0)
            return ResourceHoldResult.Invalid(errors);

        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        if (!await db.TripRequests.AnyAsync(x => x.Id == request.TripRequestId && !x.IsDeleted, cancellationToken))
            return ResourceHoldResult.NotFound("Trip request was not found.");

        var capacity = await GetCapacityAsync(type, request.ResourceId, cancellationToken);
        if (capacity is null)
            return ResourceHoldResult.NotFound("An active resource was not found.");

        if (type != "room" && request.Quantity != 1)
            return ResourceHoldResult.Invalid(new Dictionary<string, string[]>
            {
                [nameof(request.Quantity)] = ["Guide and vehicle holds must have a quantity of one."]
            });

        var reserved = await db.ResourceHolds
            .Where(x => x.ResourceType == type &&
                        x.ResourceId == request.ResourceId &&
                        x.Status == HoldStatus.Held &&
                        x.FromDate <= request.ToDate &&
                        x.ToDate >= request.FromDate)
            .SumAsync(x => x.Quantity, cancellationToken);

        if (reserved + request.Quantity > capacity.Value)
            return ResourceHoldResult.Conflict("The resource is no longer available for those dates.");

        var hold = new ResourceHold
        {
            ResourceType = type,
            ResourceId = request.ResourceId,
            TripRequestId = request.TripRequestId,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Quantity = request.Quantity
        };

        db.ResourceHolds.Add(hold);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ResourceHoldResult.Created(hold);
    }

    private async Task<int?> GetCapacityAsync(string type, Guid id, CancellationToken cancellationToken) =>
        type switch
        {
            "guide" => await db.Guides.AnyAsync(x => x.Id == id && x.IsActive && !x.IsDeleted, cancellationToken) ? 1 : null,
            "vehicle" => await db.Vehicles.AnyAsync(x => x.Id == id && x.IsActive && !x.IsDeleted, cancellationToken) ? 1 : null,
            "room" => await db.RoomTypes.Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => (int?)x.TotalRooms).SingleOrDefaultAsync(cancellationToken),
            _ => null
        };
}

public sealed record CreateResourceHold(
    string ResourceType,
    Guid ResourceId,
    Guid TripRequestId,
    DateOnly FromDate,
    DateOnly ToDate,
    int Quantity = 1);

public sealed record ResourceHoldResult(
    ResourceHold? Hold,
    string? Error,
    IReadOnlyDictionary<string, string[]>? ValidationErrors,
    ResourceHoldResultType Type)
{
    public static ResourceHoldResult Created(ResourceHold hold) => new(hold, null, null, ResourceHoldResultType.Created);
    public static ResourceHoldResult Invalid(IReadOnlyDictionary<string, string[]> errors) => new(null, null, errors, ResourceHoldResultType.Invalid);
    public static ResourceHoldResult NotFound(string error) => new(null, error, null, ResourceHoldResultType.NotFound);
    public static ResourceHoldResult Conflict(string error) => new(null, error, null, ResourceHoldResultType.Conflict);
}

public enum ResourceHoldResultType { Created, Invalid, NotFound, Conflict }
