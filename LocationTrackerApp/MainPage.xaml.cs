using LocationTrackerApp.Core.Models;
using LocationTrackerApp.Core.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Storage;

namespace LocationTrackerApp;

public partial class MainPage : ContentPage
{
    private readonly TrackingRepository _repository;
    private readonly List<TrackedLocation> _trackedLocations = [];
    private readonly List<string> _recentSummaries = [];
    private bool _isTracking;
    private IDispatcherTimer? _trackingTimer;

    public MainPage()
    {
        InitializeComponent();
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "location-tracker.db");
        _repository = new TrackingRepository(databasePath);
        RecentLocationsView.ItemsSource = _recentSummaries;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSavedLocationsAsync();
    }

    private async void OnStartTrackingClicked(object sender, EventArgs e)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status != PermissionStatus.Granted)
        {
            StatusLabel.Text = "Location access was not granted.";
            return;
        }

        _isTracking = true;
        StatusLabel.Text = "Tracking active...";
        if (_trackingTimer is null)
        {
            _trackingTimer = Dispatcher.CreateTimer();
            _trackingTimer.Interval = TimeSpan.FromSeconds(5);
            _trackingTimer.Tick += (_, _) =>
            {
                _ = TrackCurrentLocationAsync();
            };
        }

        _trackingTimer.Start();
        await TrackCurrentLocationAsync();
    }

    private void OnStopTrackingClicked(object sender, EventArgs e)
    {
        _isTracking = false;
        _trackingTimer?.Stop();
        StatusLabel.Text = "Tracking paused.";
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        _repository.ClearLocations();
        _trackedLocations.Clear();
        _recentSummaries.Clear();
        UpdateSummary();
        RenderHeatMap();
        StatusLabel.Text = "History cleared.";
    }

    private Task LoadSavedLocationsAsync()
    {
        var savedLocations = _repository.GetLocations().ToList();
        _trackedLocations.Clear();
        _trackedLocations.AddRange(savedLocations);
        RefreshRecentLocations();
        UpdateSummary();
        RenderHeatMap();
        return Task.CompletedTask;
    }

    private async Task TrackCurrentLocationAsync()
    {
        if (!_isTracking)
        {
            return;
        }

        try
        {
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10)));
            if (location is null)
            {
                StatusLabel.Text = "Location could not be read yet.";
                return;
            }

            var newRecord = new TrackedLocation
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                CapturedAt = DateTimeOffset.UtcNow
            };

            _repository.SaveLocation(newRecord);
            _trackedLocations.Add(newRecord);
            RefreshRecentLocations();
            UpdateSummary();
            RenderHeatMap();
            StatusLabel.Text = $"Saved {location.Latitude:0.0000}, {location.Longitude:0.0000}";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Tracking error: {ex.Message}";
        }
    }

    private void RefreshRecentLocations()
    {
        _recentSummaries.Clear();
        foreach (var item in _trackedLocations.TakeLast(8).Reverse())
        {
            _recentSummaries.Add($"{item.CapturedAt:HH:mm:ss} • {item.Latitude:0.0000}, {item.Longitude:0.0000}");
        }

        RecentLocationsView.ItemsSource = null;
        RecentLocationsView.ItemsSource = _recentSummaries;
    }

    private void UpdateSummary()
    {
        SummaryLabel.Text = $"Saved points: {_trackedLocations.Count}";
    }

    private void RenderHeatMap()
    {
        var heatPoints = HeatMapGenerator.BuildHeatMapPoints(_trackedLocations);
        LocationMap.Pins.Clear();

        foreach (var point in heatPoints.Take(8))
        {
            LocationMap.Pins.Add(new Pin
            {
                Label = $"Heat {point.Intensity:P0}",
                Location = new Location(point.Latitude, point.Longitude)
            });
        }

        if (_trackedLocations.Count == 0)
        {
            LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(40.7128, -74.0060), Distance.FromMiles(20)));
            return;
        }

        var lastLocation = _trackedLocations[^1];
        LocationMap.Pins.Add(new Pin
        {
            Label = "Latest location",
            Location = new Location(lastLocation.Latitude, lastLocation.Longitude)
        });

        LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(lastLocation.Latitude, lastLocation.Longitude), Distance.FromMeters(500)));
    }
}

