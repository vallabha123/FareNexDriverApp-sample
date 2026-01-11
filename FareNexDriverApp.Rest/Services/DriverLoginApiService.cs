using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Configurations;
using FareNexDriverApp.Core.Interfaces.Core.Localizations;
using FareNexDriverApp.Core.Interfaces.Rest;
using FareNexDriverApp.Core.Models.DriverLogin;
using FareNexDriverApp.Core.Utilities;
using FareNexDriverApp.Rest.ModelConverters;
using FareNexDriverApp.Rest.RestModels;
using FareNexDriverApp.Rest.RestModels.RequestModels;
using FareNexDriverApp.Rest.RestModels.ResponseModels;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;
using System.IO.Compression;

namespace FareNexDriverApp.Rest.Services
{
    public class DriverLoginApiService(HttpClient httpClient,
        IAppConfigurationsService appConfigurationsService,
        ILogService<DriverLoginApiService> logService,
        ILocalizationService localizationService) : BaseApiService(httpClient, appConfigurationsService, localizationService), IDriverLoginApiService
    {
        readonly ILogService<DriverLoginApiService> _logService = logService;
        readonly ILocalizationService _localizationService = localizationService;

        /// <summary>
        /// Http post call for login
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<bool, AppSessionData?>> LoginApi(string driverPin)
        {
            _logService.LogMethodEntry();
            Tuple<bool, AppSessionData?> responseTuple = new(false, new AppSessionData()
            {
                ErrorModel = new(string.Empty, _localizationService.GetLocalizedValue("GenericError"), ErrorDisplayType.Inline)
            });
            try
            {
                string requestUri = GetRequestUri(ConfigurationKeys.LoginApi);
                _logService.LogInformation($"Rest call - {requestUri}");
                string dateTimeStamp = DateTime.UtcNow.ToString(AppConstants.DateTimeApiFormat);
                bool isIgnitionOffWithoutEndTrip = _appConfigurationsService.IsIgnitionOffWithoutEndTrip;
                LoginRequestModel startTripRequestObj = new()
                {
                    AndroidGsfId = _appConfigurationsService.AndroidGsfId,
                    DriverPin = driverPin,
                    TimeStamp = dateTimeStamp,
                    Signature = GenerateHMACSignature(_appConfigurationsService.AndroidGsfId, driverPin, dateTimeStamp),
                    IgnitionOffDateTime = isIgnitionOffWithoutEndTrip ? _appConfigurationsService.IgnitionOffDateTime : null,
                    EndMiles = isIgnitionOffWithoutEndTrip ? _appConfigurationsService.IgnitionOffTotalMiles : null,
                };
                string requestObject = JsonConvert.SerializeObject(startTripRequestObj);
                BaseApiResponseModel apiResponse = await ExecutePostApiCallAsync(requestUri, requestObject, _logService, false);
                if (!string.IsNullOrWhiteSpace(apiResponse.ResponseContent))
                {
                    if (apiResponse.IsSuccessStatusCode)
                    {
                        LoginResponseModel? loginResponseModel = JsonConvert.DeserializeObject<LoginResponseModel>(apiResponse.ResponseContent);
                        await _appConfigurationsService.SetApiAuthToken(loginResponseModel?.Data?.Tokens?.AccessToken);
                        _appConfigurationsService.LogBusNumber = loginResponseModel?.Data?.BusNumber!;
                        _appConfigurationsService.IgnitionOffDateTime = DateTime.MinValue;
                        _appConfigurationsService.IsIgnitionOffWithoutEndTrip = false;
                        _appConfigurationsService.IgnitionOffTotalMiles = 0;
                        _appConfigurationsService.DrivenMiles = isIgnitionOffWithoutEndTrip ? 0.0 : _appConfigurationsService.DrivenMiles;
                        responseTuple = new(apiResponse.IsSuccessStatusCode, loginResponseModel?.ConvertToAppSessionModel());
                    }
                    else
                    {
                        if (apiResponse.HttpStatusCode == System.Net.HttpStatusCode.InternalServerError)
                        {
                            responseTuple = new(false, new AppSessionData()
                            {
                                ErrorModel = new(string.Empty, _localizationService.GetLocalizedValue("ServerErrorDescription"), ErrorDisplayType.Inline)
                            });
                        }
                        else if (apiResponse.ResponseContent.Contains(AppConstants.SignatureMisMatchMessage))
                        {
                            responseTuple = new(false, new AppSessionData()
                            {
                                ErrorModel = new(_localizationService.GetLocalizedValue("SystemError"), _localizationService.GetLocalizedValue("SignatureErrorDescription"), ErrorDisplayType.Popup)
                            });
                        }
                        else if (apiResponse.ResponseContent.Contains(AppConstants.InvalidDeviceMessage))
                        {
                            responseTuple = new(false, new AppSessionData()
                            {
                                ErrorModel = new(_localizationService.GetLocalizedValue("SystemError"), _localizationService.GetLocalizedValue("SystemErrorDescription"), ErrorDisplayType.Popup)
                            });
                        }
                        else if (apiResponse.ResponseContent.Contains(AppConstants.InvalidDriverPinMessage))
                        {
                            responseTuple = new(false, new AppSessionData()
                            {
                                ErrorModel = new(string.Empty, _localizationService.GetLocalizedValue("InvalidPinDescription"), ErrorDisplayType.Inline)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseTuple = new(false, new AppSessionData()
                {
                    ErrorModel = new(string.Empty, _localizationService.GetLocalizedValue("GenericError"), ErrorDisplayType.Inline)
                });
                _logService.LogException(ex);
            }
            finally
            {
                _logService.LogMethodExit();
            }
            return responseTuple;
        }

        string GenerateHMACSignature(string androidGsfId, string driverPin, string timeStamp)
        {
            string payload = $"{androidGsfId.Trim()}|{timeStamp.Trim()}|{driverPin.Trim()}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_appConfigurationsService.GetHMACSecretKey().Trim()));
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload.Trim())));
        }
    }
}


