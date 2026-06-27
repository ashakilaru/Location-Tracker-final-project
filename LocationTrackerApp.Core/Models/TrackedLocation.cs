namespace LocationTrackerApp.Core.Models;

public class TrackedLocation
{
    public int Id { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public DateTimeOffset CapturedAt { get; set; } = DateTimeOffset.UtcNow;

    public override string ToString()
    {
        return $"{Latitude:0.0000}, {Longitude:0.0000} at {CapturedAt:HH:mm:ss}";
    }
}
