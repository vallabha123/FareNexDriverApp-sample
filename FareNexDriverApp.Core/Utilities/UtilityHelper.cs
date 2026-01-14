using FareNexDriverApp.Core.Interfaces.Core.Utilities;
using FareNextDriverApp.Core.Models;
using Microsoft.Maui.Devices.Sensors;
using System;

namespace FareNextDriverApp.Core.Utilities
{
    public static class UtilityHelper
    {
        public static string ConvertDateTimeToString(
            this DateTime dateTime,
            string format = AppConstants.DateTimeFormat)
        {
            return dateTime.ToString(format);
        }

        public static Location ConvertToMauiLocation(
            this LocationModel locationModel)
        {
            return new Location()
            {
                Latitude = locationModel.Latitude,
                Longitude = locationModel.Longitude
            };
        }
    }
}
