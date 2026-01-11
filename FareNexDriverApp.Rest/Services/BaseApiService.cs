using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Configurations;
using FareNexDriverApp.Core.Interfaces.Core.Localizations;
using FareNexDriverApp.Core.Models.DriverLogin;
using FareNexDriverApp.Core.Utilities;
using FareNexDriverApp.Rest.RestModels;

namespace FareNexDriverApp.Rest.Services
{
    public class BaseApiService
    {
        protected readonly IAppConfigurationsService _appConfigurationsService;
        protected readonly HttpClient? _httpClient = null;
        readonly ILocalizationService _localizationService;


        public BaseApiService(HttpClient httpClient
            , IAppConfigurationsService appConfigurationsService,
              ILocalizationService localizationService)
        {
            _httpClient = httpClient;
            _appConfigurationsService = appConfigurationsService;
            _localizationService = localizationService;
        }

        protected async Task AttachHeadersToHttpClient()
        {
            _httpClient?.DefaultRequestHeaders.Clear();
            _httpClient?.DefaultRequestHeaders.Add(AppConstants.AuthorizationHeader, "Bearer " + await _appConfigurationsService.GetApiAuthToken());
        }

        /// <summary>
        /// Get full request uri
        /// </summary>
        /// <param name="configurationKeys"></param>
        /// <returns></returns>
        protected string GetRequestUri(ConfigurationKeys configurationKeys)
        {
            return _appConfigurationsService.GetAppConfigValue(ConfigurationKeys.ApiBaseUrl) + _appConfigurationsService.GetAppConfigValue(configurationKeys);
        }

        /// <summary>
        /// Execute post api call async
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="requestObject"></param>
        /// <param name="logService"></param>
        /// <returns></returns>
        protected async Task<BaseApiResponseModel> ExecutePostApiCallAsync<TClass>(string requestUri, string requestObject, ILogService<TClass> logService, bool addHeaders = true) where TClass : class
        {
            BaseApiResponseModel baseApiResponseModel = new();
            try
            {
                logService.LogInformation($"Rest call - Request object - {requestObject}");
                StringContent stringContent = new(requestObject, System.Text.Encoding.UTF8, AppConstants.HttpMediaType);
                if (addHeaders)
                {
                    await AttachHeadersToHttpClient();
                }
                HttpResponseMessage response = await _httpClient!.PostAsync(requestUri, stringContent);
                logService.LogInformation($"Rest call - Http response received");
                baseApiResponseModel = await ProcessHttpResponse(response, logService, baseApiResponseModel);
            }
            catch (Exception ex)
            {
                logService.LogException(ex);
            }
            finally
            {
                _httpClient?.DefaultRequestHeaders.Clear();
            }
            return baseApiResponseModel;
        }

        static async Task<BaseApiResponseModel> ProcessHttpResponse<TClass>(HttpResponseMessage httpResponseMessage, ILogService<TClass> logService, BaseApiResponseModel baseApiResponseModel) where TClass : class
        {
            baseApiResponseModel.HttpStatusCode = httpResponseMessage.StatusCode;
            baseApiResponseModel.IsSuccessStatusCode = httpResponseMessage.IsSuccessStatusCode;
            baseApiResponseModel.ResponseContent = await httpResponseMessage.Content.ReadAsStringAsync();
            logService.LogInformation($"Rest call - Response code - {(int)httpResponseMessage.StatusCode}\n Response content - {baseApiResponseModel.ResponseContent}");
            return baseApiResponseModel;
        }

        protected void ValidateApiResponseContent(BaseApiResponseModel apiResponse, bool isStartTrip, ref Tuple<bool, ErrorModel?> responseTuple)
        {
            if (apiResponse!.ResponseContent!.Contains(AppConstants.SessionExpiredMessage)
                || apiResponse.ResponseContent.Contains(AppConstants.InvalidTokenMessage))
            {
                responseTuple = new(false, new(_localizationService?.GetLocalizedValue("SessionExpired"), _localizationService?.GetLocalizedValue("SessionExpiredDescription"), ErrorDisplayType.Popup));
            }
            else if (apiResponse.HttpStatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                responseTuple = new(false, new(_localizationService?.GetLocalizedValue("ServerError"), _localizationService?.GetLocalizedValue("ServerDownErrorDescription"), ErrorDisplayType.Popup));
            }
            else if (apiResponse.HttpStatusCode == System.Net.HttpStatusCode.BadRequest
                || apiResponse.HttpStatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                responseTuple = new(false, new(_localizationService?.GetLocalizedValue("ServerError"), _localizationService?.GetLocalizedValue(isStartTrip ? "TripFailedDescription" : "EndTripFailedDescription"), ErrorDisplayType.Popup));
            }
        }
    }
}