using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FareNexDriverApp.Core.Interfaces.Core.Utilities
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
        public const string ApplicationNewLine = "\n------------------------------------------------------------";
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
    }
}
