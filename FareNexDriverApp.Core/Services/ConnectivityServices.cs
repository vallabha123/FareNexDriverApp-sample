using FareNexDriverApp.Core.Interfaces.Core;
using Microsoft.Maui.Networking;
using System;
using System.Linq;

namespace FareNextDriverApp.Core.Services
{
    public sealed class ConnectivityService : IConnectivityService, IDisposable
    {
        private readonly IConnectivity _connectivity;
        private readonly ILogService<ConnectivityService> _logService;
        private bool IsConnected;

        public event EventHandler<bool>? OnConnectionStatusChanged;

        public ConnectivityService(
            IConnectivity connectivity,
            ILogService<ConnectivityService> logService)
        {
            _connectivity = connectivity;
            _logService = logService;
        }

        public void StartConnectivityListener()
        {
            _logService.LogMethodEntry();

            _connectivity.ConnectivityChanged -= Current_ConnectivityChanged;
            _connectivity.ConnectivityChanged += Current_ConnectivityChanged;

            Current_ConnectivityChanged(
                this,
                new ConnectivityChangedEventArgs(
                    _connectivity.NetworkAccess,
                    _connectivity.ConnectionProfiles));

            _logService.LogMethodExit("Connection Change Listener Started");
        }

        private void Current_ConnectivityChanged(
            object? sender,
            ConnectivityChangedEventArgs e)
        {
            IsConnected =
                e.NetworkAccess == NetworkAccess.Internet &&
                e.ConnectionProfiles.Any(p =>
                    p == ConnectionProfile.WiFi ||
                    p == ConnectionProfile.Cellular ||
                    p == ConnectionProfile.Ethernet);

            OnConnectionStatusChanged?.Invoke(this, IsConnected);

            _logService.LogInformation(
                "Network Connection Status Changed - " + IsConnected);
        }

        /// <summary>
        /// Check if device is online or offline
        /// </summary>
        /// <returns>true if network available</returns>
        public bool IsNetworkAvailable() => IsConnected;

        public void Dispose()
        {
            _connectivity.ConnectivityChanged -= Current_ConnectivityChanged;
            GC.SuppressFinalize(this);
        }
    }
}
