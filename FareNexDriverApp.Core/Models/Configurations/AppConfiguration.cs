using FareNexDriverApp.Core.Utilities;
using System.Collections.Generic;

namespace FareNexDriverApp.Core.Models.Configurations
{
    public static class AppConfiguration
    {
        public static Dictionary<ConfigurationKeys, string> ConfigurationValues { get; private set; }
            = new()
            {
                { ConfigurationKeys.SerilogLogFileName, "Serilog:LogFileName" },
                { ConfigurationKeys.ApiBaseUrl, "Api:BaseUrl" },
                { ConfigurationKeys.LoginApi, "Api:Login" },
                { ConfigurationKeys.StartTripApi, "Api:StartTrip" },
                { ConfigurationKeys.HMACSecretKey, "SecretKeys:HMACSecret" },
                { ConfigurationKeys.EndTripApi, "Api:EndTrip" },
                { ConfigurationKeys.SseEvents, "Api:SseEvents" },
                { ConfigurationKeys.LogUploadApi, "Api:LogUpload" },
                { ConfigurationKeys.LogUploadKey, "SecretKeys:LogUploadKey" }
            };
    }
}
