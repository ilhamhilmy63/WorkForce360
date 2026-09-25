namespace WorkForce360.Api.Services;

public static class ResourceAvailabilityRules
{
    public static bool DatesOverlap(
        DateOnly existingFrom,
        DateOnly existingTo,
        DateOnly requestedFrom,
        DateOnly requestedTo) =>
        existingFrom <= requestedTo && existingTo >= requestedFrom;

    public static IReadOnlyDictionary<string, string[]> ValidateHold(
        string resourceType,
        DateOnly from,
        DateOnly to,
        int quantity)
    {
        var errors = new Dictionary<string, string[]>();

        if (!SupportedTypes.Contains(resourceType, StringComparer.OrdinalIgnoreCase))
            errors[nameof(resourceType)] = ["Resource type must be guide, vehicle, or room."];

        if (to < from)
            errors[nameof(to)] = ["The end date must be on or after the start date."];

        if (quantity < 1)
            errors[nameof(quantity)] = ["Quantity must be at least one."];

        return errors;
    }

    public static readonly string[] SupportedTypes = ["guide", "vehicle", "room"];
}
