using FareNexDriverApp.Core.EventArgs;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FareNexDriverApp.Core.Interfaces.Core
{
    public interface ILocationService
    {
        bool IsListeningForeground { get; }

        event EventHandler<GeoLocationMonitorEventArgs>? LocationChanged;

        Task<Location?> GetLastKnownLocationAsync();

        Task<Location?> GetLocationAsync(
            GeolocationRequest request,
            CancellationToken cancelToken);

        Task<bool> StartListeningForegroundAsync(
            GeolocationListeningRequest request);

        void StopListeningForeground();
    }
}
