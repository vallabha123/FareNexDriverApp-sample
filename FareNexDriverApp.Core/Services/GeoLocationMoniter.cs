using FareNexDriverApp.Core.EventArgs;
using FareNexDriverApp.Core.Interfaces;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Configurations;
using FareNexDriverApp.Core.Models;
using FareNexDriverApp.Core.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FareNexDriverApp.Core.Services
{
    public class GeoLocationMonitor
    {
        private ILogService<GeoLocationMonitor>? _logService;
        private IAppConfigurationsService? _appConfigurationsService;

        public readonly static GeoLocationMonitor Instance = new();

        private LocationModel? previousLoc;

        private const int AccuracyInMeters = 10;
        private const int MinSpeedInKmH = 3;

        private string _filePath = string.Empty;

        /// <summary>
        /// Starts background task for capturing location updates
        /// </summary>
        public async Task StartCapturingLocationBreadCrumbsAsync(
            ILogService<GeoLocationMonitor> logService,
            ILocationService locationService,
            IAppConfigurationsService appConfigurationsService)
        {
            await Task.Delay(100); // Ensure UI loaded

            _ = Task.Run(async () =>
            {
                _logService = logService;
                _appConfigurationsService = appConfigurationsService;

                _logService?.LogMethodEntry();

                try
                {
                    if (locationService == null)
                        throw new InvalidOperationException("Location Service cannot be null");

                    if (locationService.IsListeningForeground)
                        throw new InvalidOperationException("Location service is already running");

                    if (await locationService.StartListeningForegroundAsync(new()))
                    {
                        _filePath = Path.Combine(
                            AppConstants.AppExternalDataFolderPath(),
                            AppConstants.AppLogDataFolderName,
                            AppConstants.GpsBreadCrumbsFileName);

                        if (!File.Exists(_filePath))
                        {
                            Directory.CreateDirectory(
                                Path.GetDirectoryName(_filePath)!);

                            File.Create(_filePath).Close();
                        }

                        locationService.LocationChanged -= FusedLocationService_LocationChanged;
                        locationService.LocationChanged += FusedLocationService_LocationChanged;

                        _logService?.LogInformation("GPS Location Tracking Started");
                    }
                }
                catch (Exception ex)
                {
                    _logService?.LogException(ex);
                }

                _logService?.LogMethodExit();
            });
        }

        /// <summary>
        /// Raised when there is a location change
        /// </summary>
        private async void FusedLocationService_LocationChanged(
            object? sender,
            GeoLocationMonitorEventArgs e)
        {
            await Task.Run(async () =>
            {
                try
                {
                    if (e.Location.Accuracy > AccuracyInMeters ||
                        e.Location.Speed < MinSpeedInKmH)
                    {
                        _logService?.LogInformation(
                            "Accuracy and speed conditions not met - " +
                            $"{e.Location.Accuracy} - {e.Location.Speed} Km/H");

                        return;
                    }

                    if (previousLoc is not null)
                    {
                        double distance = Location.CalculateDistance(
                            previousLoc.ConvertToMauiLocation(),
                            e.Location.ConvertToMauiLocation(),
                            DistanceUnits.Miles);

                        _appConfigurationsService!.DrivenMiles += distance;
                        e.Location.DistanceInMiles = distance;
                    }

                    previousLoc = e.Location;

                    string json =
                        AppConstants.GpsBreadCrumbsDelimiter +
                        JsonSerializer.Serialize(e.Location);

                    Debug.WriteLine(json);

                    _logService?.LogInformation(
                        "Total Distance in Miles - " +
                        _appConfigurationsService!.DrivenMiles +
                        " Speed - " + e.Location.Speed);

                    using StreamWriter writer =
                        new(_filePath, append: true)
                        {
                            AutoFlush = true
                        };

                    await writer.WriteLineAsync(json);
                    await writer.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logService?.LogException(ex);
                }
            });
        }
    }
}
