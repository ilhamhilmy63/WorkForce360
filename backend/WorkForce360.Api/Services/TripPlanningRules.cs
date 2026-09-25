using WorkForce360.Api.Models;

namespace WorkForce360.Api.Services;

public class TripPlanningRules
{
    public IReadOnlyList<string> Validate(DateOnly startDate, DateOnly endDate, int pax, decimal budgetUsd, string objective)
    {
        var errors = new List<string>();
        if (endDate < startDate) errors.Add("End date must be on or after the start date.");
        if (pax < 1) errors.Add("Passenger count must be at least one.");
        if (budgetUsd <= 0) errors.Add("Budget must be greater than zero.");
        if (string.IsNullOrWhiteSpace(objective)) errors.Add("Trip objective is required.");
        if (startDate.DayNumber <= endDate.DayNumber && endDate.DayNumber - startDate.DayNumber + 1 > 30) errors.Add("A trip may not exceed 30 days.");
        return errors;
    }

    public IReadOnlyList<ItineraryDay> BuildSkeleton(DateOnly startDate, DateOnly endDate)
    {
        var count = endDate.DayNumber - startDate.DayNumber + 1;
        return Enumerable.Range(1, count).Select(day => new ItineraryDay
        {
            DayNumber = day,
            City = "To be planned",
            Notes = "Awaiting itinerary planning"
        }).ToList();
    }
}
