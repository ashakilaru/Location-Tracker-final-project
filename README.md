# Location Tracker Final Project

This repository contains a .NET MAUI application that records the user’s location, stores it in SQLite, and displays recent points in a map-based visualization styled as a heat-map view.

## What is included
- MAUI app UI with start/stop tracking controls
- SQLite-backed storage for captured positions
- Shared core logic for location persistence and heat-map clustering
- Unit tests for repository and heat-map behavior
- Screenshot and Word submission artifacts in the docs folder

## Run locally
1. Open the solution file in Visual Studio or VS Code.
2. Build and run the MAUI app on a supported platform.
3. Grant location permission when prompted.

## Verification
The project was verified with:
- dotnet test LocationTrackerApp.Tests/LocationTrackerApp.Tests.csproj
- dotnet build LocationTrackerApp/LocationTrackerApp.csproj -f net8.0-maccatalyst
