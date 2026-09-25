using WorkForce360.Api.Services;

namespace WorkForce360.Api.Tests;

public class TripPlanningRulesTests
{
    private readonly TripPlanningRules _rules = new();

    [Fact]
    public void Validate_AcceptsValidFiveDayTrip()
    {
        var errors = _rules.Validate(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 14), 4, 1500, "Kandy and Ella");
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_RejectsInvalidDatesPassengersAndBudget()
    {
        var errors = _rules.Validate(new DateOnly(2026, 10, 14), new DateOnly(2026, 10, 10), 0, 0, "");
        Assert.Equal(4, errors.Count);
    }

    [Fact]
    public void BuildSkeleton_CreatesOneOrderedDayPerTravelDate()
    {
        var days = _rules.BuildSkeleton(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 14));
        Assert.Equal(5, days.Count);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, days.Select(x => x.DayNumber));
    }
}
