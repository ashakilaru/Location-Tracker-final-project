using System.Data;
using LocationTrackerApp.Core.Models;
using Microsoft.Data.Sqlite;

namespace LocationTrackerApp.Core.Services;

public class TrackingRepository
{
    private readonly string _databasePath;

    public TrackingRepository(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        _databasePath = databasePath;
        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);
        InitializeDatabase();
    }

    public void SaveLocation(TrackedLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO TrackedLocations (Latitude, Longitude, CapturedAt)
            VALUES ($latitude, $longitude, $capturedAt);";
        command.Parameters.AddWithValue("$latitude", location.Latitude);
        command.Parameters.AddWithValue("$longitude", location.Longitude);
        command.Parameters.AddWithValue("$capturedAt", location.CapturedAt.UtcDateTime.ToString("O"));
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<TrackedLocation> GetLocations()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Latitude, Longitude, CapturedAt
            FROM TrackedLocations
            ORDER BY CapturedAt ASC;";

        using var reader = command.ExecuteReader();
        var locations = new List<TrackedLocation>();
        while (reader.Read())
        {
            locations.Add(new TrackedLocation
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Latitude = reader.GetDouble(reader.GetOrdinal("Latitude")),
                Longitude = reader.GetDouble(reader.GetOrdinal("Longitude")),
                CapturedAt = DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("CapturedAt")))
            });
        }

        return locations;
    }

    public void ClearLocations()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM TrackedLocations;";
        command.ExecuteNonQuery();
    }

    private void InitializeDatabase()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS TrackedLocations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Latitude REAL NOT NULL,
                Longitude REAL NOT NULL,
                CapturedAt TEXT NOT NULL
            );";
        command.ExecuteNonQuery();
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();
        return connection;
    }
}
