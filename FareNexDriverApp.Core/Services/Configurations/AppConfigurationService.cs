using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Configurations;
using FareNexDriverApp.Core.Models.Configurations;
using FareNexDriverApp.Core.Models.DriverLogin;
using FareNexDriverApp.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace FareNexDriverApp.Core.Services.Configurations
{
    public class AppConfigurationsService : IAppConfigurationsService
    {
        private readonly IConfiguration _configuration;
        private readonly ISecureStorage _secureStorage;
        private readonly IPreferences _preferences;

        private readonly string ApiAuthToken = nameof(ApiAuthToken);
        private readonly string HMACSecretKey = nameof(HMACSecretKey);

        public AppConfigurationsService(
            IConfiguration configuration,
            ISecureStorage secureStorage,
            IPreferences preferences)
        {
            _configuration = configuration;
            _secureStorage = secureStorage;
            _preferences = preferences;
        }

        /// <summary>
        /// Application session object
        /// </summary>
        public AppSessionData AppSessionDataObj { get; set; } = new();

        /// <summary>
        /// Get android gsf id
        /// </summary>
        public string AndroidGsfId
        {
            get => _preferences.Get(AppConstants.AndroidGsfIdKey, string.Empty);
            set => _preferences.Set(AppConstants.AndroidGsfIdKey, value);
        }

        /// <summary>
        /// Get config values from json config files
        /// </summary>
        public string GetAppConfigValue(ConfigurationKeys key)
        {
            return _configuration[AppConfiguration.ConfigurationValues[key]] ?? string.Empty;
        }

        /// <summary>
        /// Get API auth token
        /// </summary>
        public async Task<string?> GetApiAuthToken()
        {
            return await _secureStorage.GetAsync(ApiAuthToken);
        }

        /// <summary>
        /// Set API auth token
        /// </summary>
        public async Task SetApiAuthToken(string? value)
        {
            if (value != null)
            {
                await _secureStorage.SetAsync(ApiAuthToken, value);
            }
        }

        /// <summary>
        /// Get HMAC secret key
        /// </summary>
        public string GetHMACSecretKey()
        {
            return GetAppConfigValue(ConfigurationKeys.HMACSecretKey);
        }

        /// <summary>
        /// Get driven miles
        /// </summary>
        public double DrivenMiles
        {
            get => _preferences.Get(AppConstants.DrivenMilesKey, 0.0);
            set => _preferences.Set(AppConstants.DrivenMilesKey, value);
        }

        /// <summary>
        /// Clear session & cached data
        /// </summary>
        public async Task ClearAndResetCacheData()
        {
            DrivenMiles = 0.0;
            AppSessionDataObj = new AppSessionData();
            await SetApiAuthToken(string.Empty);
        }

        /// <summary>
        /// Get bus number for log upload
        /// </summary>
        public string LogBusNumber
        {
            get => _preferences.Get(AppConstants.LogBusNumberKey, string.Empty);
            set => _preferences.Set(AppConstants.LogBusNumberKey, value);
        }

        /// <summary>
        /// Get or set if bus is turned off before end trip
        /// </summary>
        public bool IsIgnitionOffWithoutEndTrip
        {
            get => _preferences.Get(AppConstants.IsIgnitionOffWithoutEndTrip, false);
            set => _preferences.Set(AppConstants.IsIgnitionOffWithoutEndTrip, value);
        }

        /// <summary>
        /// Get or set ignition off date time
        /// </summary>
        public DateTime IgnitionOffDateTime
        {
            get => _preferences.Get(AppConstants.IgnitionOffDateTime, DateTime.MinValue);
            set => _preferences.Set(AppConstants.IgnitionOffDateTime, value);
        }

        /// <summary>
        /// Get or set ignition off total miles
        /// </summary>
        public double IgnitionOffTotalMiles
        {
            get => _preferences.Get(AppConstants.IgnitionOffTotalMiles, 0.0);
            set => _preferences.Set(AppConstants.IgnitionOffTotalMiles, value);
        }
    }
}
