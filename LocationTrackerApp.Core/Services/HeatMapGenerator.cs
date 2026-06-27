using LocationTrackerApp.Core.Models;

namespace LocationTrackerApp.Core.Services;

public static class HeatMapGenerator
{
    public static IReadOnlyList<HeatMapPoint> BuildHeatMapPoints(IEnumerable<TrackedLocation> locations)
    {
        ArgumentNullException.ThrowIfNull(locations);

        return locations
            .GroupBy(location => new
            {
                Latitude = Math.Round(location.Latitude, 2),
                Longitude = Math.Round(location.Longitude, 2)
            })
            .Select(group => new HeatMapPoint(
                group.Key.Latitude,
                group.Key.Longitude,
                Math.Min(1.0, group.Count() / 10.0)))
            .OrderByDescending(point => point.Intensity)
            .ToList();
    }
}

public sealed record HeatMapPoint(double Latitude, double Longitude, double Intensity);
