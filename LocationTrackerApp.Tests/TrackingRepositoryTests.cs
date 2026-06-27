using LocationTrackerApp.Core.Models;
using LocationTrackerApp.Core.Services;

namespace LocationTrackerApp.Tests;

public class TrackingRepositoryTests
{
    [Fact]
    public void SaveAndReadLocationsRoundTrip()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"tracker-{Guid.NewGuid():N}.db");
        try
        {
            var repository = new TrackingRepository(databasePath);
            repository.SaveLocation(new TrackedLocation { Latitude = 45.5, Longitude = -73.6, CapturedAt = new DateTimeOffset(2026, 6, 26, 19, 0, 0, TimeSpan.Zero) });
            repository.SaveLocation(new TrackedLocation { Latitude = 45.51, Longitude = -73.61, CapturedAt = new DateTimeOffset(2026, 6, 26, 19, 5, 0, TimeSpan.Zero) });

            var saved = repository.GetLocations();

            Assert.Equal(2, saved.Count);
            Assert.Equal(45.5, saved[0].Latitude);
            Assert.Equal(-73.6, saved[0].Longitude);
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    [Fact]
    public void HeatMapGeneratorAggregatesNearbyCoordinates()
    {
        var locations = new[]
        {
            new TrackedLocation { Latitude = 45.5001, Longitude = -73.6001 },
            new TrackedLocation { Latitude = 45.5002, Longitude = -73.6002 },
            new TrackedLocation { Latitude = 45.5010, Longitude = -73.6010 }
        };

        var points = HeatMapGenerator.BuildHeatMapPoints(locations);

        Assert.Single(points);
        Assert.Equal(0.3, points[0].Intensity, 3);
    }
}
