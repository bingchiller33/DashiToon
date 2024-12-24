using DashiToon.Api.Application.AuthorStudio.Analytics.Models;

namespace DashiToon.Api.Application.AuthorStudio.Analytics;

public static class DateRangeUtils
{
    /// <summary>
    /// Ensure the time range between From and To of the 'current' date range and optionally 'compare' date range are the same, and update date ranges if there are differences 
    /// </summary>
    /// <param name="current">The current date range</param>
    /// <param name="compare">Optional compare date range</param>
    public static void UpdateDateRange(DateRange current, DateRange? compare)
    {
        if (compare is not null)
        {
            TimeSpan currentRangeTimeDiff = current.To - current.From;

            TimeSpan compareRangeTimeDiff = compare.To - compare.From;

            if (compareRangeTimeDiff > currentRangeTimeDiff)
            {
                current.To = current.From + compareRangeTimeDiff;
            }

            if (currentRangeTimeDiff > compareRangeTimeDiff)
            {
                compare.To = compare.From + currentRangeTimeDiff;
            }

            if (compare.From == compare.To)
            {
                compare.To = compare.To.AddDays(1);
            }
        }

        if (current.From == current.To)
        {
            current.To = current.To.AddDays(1);
        }
    }
}
