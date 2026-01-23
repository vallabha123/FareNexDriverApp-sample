using System;
using System.IO;

namespace FareNexDriverApp.Core.Utilities
{
    public static class AppConstants
    {
        public const string LangEnUs = "en-US";

        public const string DateTimeFormat = "dd-MMM-yyyy HH:mm:ss";
        public const string DateTimeDashboardFormat = "dddd MMM dd | hh:mm tt";

        public const string AnimationIdentifierName = "WidthAnimation";
        public const string AnimationMessageStart = "StartAnimation";
        public const string AnimationMessageCancel = "CancelAnimation";
        public const string AnimationMessageCancelAndUnregister = "CancelAnimationAndUnregister";

        public const string DateTimeApiFormat = "yyyy-MM-ddTHH:mm:ssZ";

        public const string LocalFareCategory = "Local";
        public const string RegionalFareCategory = "Regional";

        public const string AndroidGsfIdKey = nameof(AndroidGsfIdKey);
        public const string GsfIdContentUri = "content://com.google.android.gsf.gservices";

        public const string ApplicationNewLine =
            "\n------------------------------------------------------------";

        public const string PopupExceptionMessage = "An active popup already exists.";

        public const string SignatureMisMatchMessage = "Invalid Signature";
        public const string InvalidDeviceMessage = "Invalid Android GSF ID";
        public const string InvalidDriverPinMessage = "Invalid driver pin";

        public const string GpsBreadCrumbsFileName = "GpsBreadCrumbsFareNex.txt";
        public const string AppLogDataFolderName = "FareNexLogs";

        public const char GpsBreadCrumbsDelimiter = '|';

        public const string DrivenMilesKey = nameof(DrivenMilesKey);

        public const string SessionExpiredMessage = "Session expired. Please log in again.";
        public const string InvalidTokenMessage = "Invalid token.";

        public const string FareNexLogZipFileName = "{0}.zip";
        public const string TempFolder = "Temp";

        public const string LogBusNumberKey = nameof(LogBusNumberKey);

        public const string IgnitionOffDateTime = nameof(IgnitionOffDateTime);
        public const string IsIgnitionOffWithoutEndTrip = nameof(IsIgnitionOffWithoutEndTrip);
        public const string IgnitionOffTotalMiles = nameof(IgnitionOffTotalMiles);

        /// <summary>
        /// Get application external data folder Android/Data/app identifier
        /// Else returns local app data for use in xunit tests
        /// </summary>
        public static string AppExternalDataFolderPath()
        {
#if ANDROID
            return Platform.AppContext.GetExternalFilesDir(null)?.AbsolutePath ?? string.Empty;
#else
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FareNexUnitTest");
#endif
        }

        #region LogService Strings

        public const string LogExtensionString =
            "{MemberName} | {Name}";

        public const string LogExtensionInformationString =
            "{FormattedString} | {MemberName} | {Name}";

        public const string LogExtensionEntryString =
            "Entered - {MemberName} | {Name}";

        public const string LogExtensionEntryStringExtra =
            "Entered - {FormattedString} | {MemberName} | {Name}";

        public const string LogExtensionExitString =
            "Exit - {MemberName} | {Name}";

        public const string LogExtensionExitStringExtra = "Exit - {FormattedString} | {MemberName} | {Name}";

        #endregion

        #region Rest Service Strings

        public const string HttpMediaType = "application/json";
        public const string CorrelationHeader = "X-Correlation-ID";
        public const string AuthorizationHeader = "Authorization";

        public const string StartTripSuccessMessage = "Trip successfully started";

        public const string EndTripSuccessMessage = "Trip ended successfully";

        public const string HttpMultiPartMediaType = "multipart/form-data";

        public const string HttpMultiPartZipMediaType ="application/zip";

        #endregion
    }
}
