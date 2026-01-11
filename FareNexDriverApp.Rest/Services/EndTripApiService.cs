using FareNexDriverApp.Rest.Services;
?using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Configurations;
using FareNexDriverApp.Core.Interfaces.Core.Localizations;
using FareNexDriverApp.Core.Interfaces.Rest;
using FareNexDriverApp.Core.Models.DriverLogin;
using FareNexDriverApp.Core.Utilities;
using FareNexDriverApp.Rest.RestModels;
using FareNexDriverApp.Rest.RestModels.RequestModels;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;
using System.IO.Compression;

namespace FareNexDriverApp.Rest.Services
{
    public class EndTripApiService(HttpClient httpClient,
        IAppConfigurationsService appConfigurationsService,
        ILogService<EndTripApiService> logService,
        ILocalizationService localizationService,
        ILogUploadApiService logUploadApiService) : BaseApiService(httpClient, appConfigurationsService, localizationService), IEndTripApiService
    {
        readonly ILogService<EndTripApiService> _logService = logService;
        readonly ILocalizationService? _localizationService = localizationService;
        readonly ILogUploadApiService? _logUploadApiService = logUploadApiService;

        /// <summary>
        /// Http post call for end trip
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<bool, ErrorModel?>> EndTripApi(double endMiles)
        {
            _logService.LogMethodEntry();
            Tuple<bool, ErrorModel?> responseTuple = new(false, new(_localizationService?.GetLocalizedValue("ApplicationError"), _localizationService?.GetLocalizedValue("GenericError"), ErrorDisplayType.Popup));
            try
            {
                string requestUri = GetRequestUri(ConfigurationKeys.EndTripApi);
                _logService.LogInformation($"Rest call - {requestUri}");
                EndTripRequestModel endTripRequestObj = new()
                {
                    BusNumber = _appConfigurationsService.AppSessionDataObj?.BusObj?.BusNumber!,
                    BusId = _appConfigurationsService.AppSessionDataObj?.BusObj?.BusId!.ToString()!,
                    EndMiles = Math.Round(_appConfigurationsService.AppSessionDataObj!.StartMiles + endMiles, 2),
                    AndroidGsfId = _appConfigurationsService.AndroidGsfId,
                    EndTripTimeStamp = DateTime.UtcNow
                };
                string requestObject = JsonConvert.SerializeObject(endTripRequestObj);
                BaseApiResponseModel apiResponse = await ExecutePostApiCallAsync(requestUri, requestObject, _logService, true);
                if (!string.IsNullOrEmpty(apiResponse.ResponseContent))
                {
                    if (apiResponse.IsSuccessStatusCode &&
                        apiResponse.ResponseContent.Contains(AppConstants.EndTripSuccessMessage))
                    {
                        responseTuple = new(true, null);
                        _appConfigurationsService.DrivenMiles = 0.0;
                        await _logUploadApiService!.UploadLogsAsync(_appConfigurationsService.AppSessionDataObj?.TripId);
                    }
                    else
                    {
                        ValidateApiResponseContent(apiResponse, false, ref responseTuple);
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