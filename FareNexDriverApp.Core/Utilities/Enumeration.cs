using System.ComponentModel.DataAnnotations;

namespace FareNexDriverApp.Core.Utilities
{
    public enum ConfigurationKeys
    {
        [Display(Description = "Serilog Log File Name")]
        SerilogLogFileName,

        ApiBaseUrl,
        LoginApi,
        StartTripApi,
        HMACSecretKey,
        EndTripApi,
        SseEvents,
        LogUploadApi,
        LogUploadKey
    }

    public enum ErrorDisplayType
    {
        Inline,
        Popup,
        Banner
    }

    /// <summary>
    /// Represents type of Passenger.
    /// </summary>
    public enum PassengerType
    {
        Adult,
        Child,
        SeniorCitizen
    }
}
