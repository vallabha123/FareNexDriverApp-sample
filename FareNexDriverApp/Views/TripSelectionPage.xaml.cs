using Android.Mtp;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Configurations;
using FareNexDriverApp.Core.Interfaces.Core.Localizations;
using FareNexDriverApp.Core.Interfaces.Rest;
using FareNexDriverApp.Core.Models.DriverLogin;
using FareNexDriverApp.Core.Utilities;
using FareNexDriverApp.Rest.RestModels;
using FareNexDriverApp.Rest.RestModels.RequestModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FareNexDriverApp.Rest.Services
{
    public class TripSelectionApiService(HttpClient httpClient,
        IAppConfigurationsService appConfigurationsService,
        ILogService<TripSelectionApiService> logService,
        ILocalizationService localizationService) : BaseApiService(httpClient, appConfigurationsService, localizationService), ITripSelectionApiService
    {
        readonly ILogService<TripSelectionApiService> _logService = logService;
        readonly ILocalizationService? _localizationService = localizationService;

        /// <summary>
        /// Http post call for start trip
        /// </summary>
        /// <param name="selectedRouteID"></param>
        /// <returns></returns>
        public async Task<Tuple<bool, ErrorModel?>> StartTripApi(string? selectedRouteID, string? fareCategory)
        {
            _logService.LogMethodEntry();
            Tuple<bool, ErrorModel?> responseTuple = new(false, new(_localizationService?.GetLocalizedValue("ApplicationError"), _localizationService?.GetLocalizedValue("GenericError"), ErrorDisplayType.Popup));
            try
            {
                string requestUri = GetRequestUri(ConfigurationKeys.StartTripApi);
                _logService.LogInformation($"Rest call - {requestUri}");
                StartTripRequestModel startTripRequestObj = new()
                {
                    BusId = _appConfigurationsService.AppSessionDataObj.BusObj!.BusId!,
                    RouteId = selectedRouteID!,
                    DriverId = _appConfigurationsService.AppSessionDataObj.DriveInfoObj!.DriverId!,
                    AndroidGsfId = _appConfigurationsService.AndroidGsfId,
                    RouteType = fareCategory!
                };
                string requestObject = JsonConvert.SerializeObject(startTripRequestObj);
                BaseApiResponseModel apiResponse = await ExecutePostApiCallAsync(requestUri, requestObject, _logService, true);
                if (!string.IsNullOrEmpty(apiResponse.ResponseContent))
                {
                    if (apiResponse.IsSuccessStatusCode &&
                        apiResponse.ResponseContent.Contains(AppConstants.StartTripSuccessMessage))
                    {
                        JObject keyValuePairs = JObject.Parse(apiResponse.ResponseContent);
                        _appConfigurationsService.AppSessionDataObj.TripId = keyValuePairs["data"]?["tripId"]?.ToString();
                        responseTuple = new(true, null);
                    }
                    else
                    {
                        ValidateApiResponseContent(apiResponse, true, ref responseTuple);
                    }
                }
            }
            catch (Exception ex)
            {
                responseTuple = new(false, new(_localizationService?.GetLocalizedValue("ApplicationError"), _localizationService?.GetLocalizedValue("GenericError"), ErrorDisplayType.Popup));
                _logService.LogException(ex);
            }
            finally
            {
                _logService.LogMethodExit();
            }
            return responseTuple;
        }
    }
}
