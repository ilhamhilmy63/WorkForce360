using WorkForce360.Api.Services;

namespace WorkForce360.Api.Tests;

public class ResourceAvailabilityRulesTests
{
    [Theory]
    [InlineData("2026-10-01", "2026-10-03", "2026-10-03", "2026-10-05", true)]
    [InlineData("2026-10-01", "2026-10-03", "2026-10-04", "2026-10-05", false)]
    [InlineData("2026-10-02", "2026-10-06", "2026-10-03", "2026-10-04", true)]
    public void DatesOverlap_DetectsInclusiveDateCollisions(
        string existingFrom,
        string existingTo,
        string requestedFrom,
        string requestedTo,
        bool expected)
    {
        var result = ResourceAvailabilityRules.DatesOverlap(
            DateOnly.Parse(existingFrom),
            DateOnly.Parse(existingTo),
            DateOnly.Parse(requestedFrom),
            DateOnly.Parse(requestedTo));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidateHold_ReturnsErrorsForInvalidInput()
    {
        var errors = ResourceAvailabilityRules.ValidateHold(
            "aircraft",
            new DateOnly(2026, 10, 5),
            new DateOnly(2026, 10, 3),
            0);

        Assert.Contains("resourceType", errors.Keys);
        Assert.Contains("to", errors.Keys);
        Assert.Contains("quantity", errors.Keys);
    }
}
