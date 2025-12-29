using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Configurations;
using FareNexDriverApp.Core.Interfaces.Core.Localizations;
using FareNexDriverApp.Core.Interfaces.Rest;
using FareNexDriverApp.Core.Models.DriverLogin;
using FareNexDriverApp.Core.ViewModels.BaseViewModels;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace FareNexDriverApp.Core.ViewModels
{
    public partial class EndTripPageViewModel : BaseNavigationViewModel
    {
        #region Properties

        private readonly ILogService<EndTripPageViewModel> _logService;
        private readonly ILocationService _locationService;

        [ObservableProperty]
        private string? _busNumber;

        [ObservableProperty]
        private string? _selectedRoute;

        [ObservableProperty]
        private string? _startMiles;

        [ObservableProperty]
        private string? _endMiles;

        [ObservableProperty]
        private string? _drivenMiles;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logService"></param>
        public EndTripPageViewModel(IServiceProvider serviceProvider,
            IPopupService popupService,
            ILogService<EndTripPageViewModel> logService,
            INavigationService navigationService,
            IAppConfigurationsService appConfigurationsService,
            IEndTripApiService endTripApiService,
            IConnectivityService connectivityService,
            ILocationService locationService,
            ILocalizationService localizationService,
            ISseEventsApiService sseEventsApiService) : base(serviceProvider, popupService, connectivityService)
        {
            _logService = logService;
            _navigationService = navigationService;
            _endTripApiService = endTripApiService;
            _appConfigurationsService = appConfigurationsService;
            _connectionService = connectivityService;
            _locationService = locationService;
            _localizationService = localizationService;
            _sseEventsApiService = sseEventsApiService;
            _logService.LogMethodEntry();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// End Ride Command
        /// </summary>
        [RelayCommand]
        public async Task EndRide()
        {
            if (!IsLoadingIndicatorRunning && IsNetworkAvailable)
            {
                await ShowLoadingIndicator();
                _logService.LogMethodEntry();
                try
                {
                    RaiseExceptionForTests();
                    _locationService.StopListeningForeground();
                    _logService.LogInformation("GPS Location Tracking ended - " + !_locationService.IsListeningForeground + Environment.NewLine + "Driven Miles - " + _appConfigurationsService!.DrivenMiles);
                    Tuple<bool, ErrorModel?> resTuple = await _endTripApiService!.EndTripApi(Math.Round(_appConfigurationsService!.DrivenMiles, 2));
                    if (resTuple is not null
                        && resTuple.Item1)
                    {
                        await _appConfigurationsService!.ClearAndResetCacheData();
                        await Task.Delay(50);
                        _navigationService!.NavigateToMainPage();
                    }
                    else
                    {
                        string errorContent = resTuple?.Item2?.ErrorMessage!;
                        string title = resTuple?.Item2?.ErrorTitle!;
                        await HideLoadingIndicator();
                        await Task.Delay(50);
                        await _popupService!.ShowPopupAsync(new Models.CustomModels.AlertPopupModel(title, errorContent));
                        if (title == _localizationService?.GetLocalizedValue("SessionExpired"))
                        {
                            _navigationService?.NavigateToMainPage();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex);
                }
                finally
                {
                    await HideLoadingIndicator();
                    _logService.LogMethodExit();
                }
            }
        }

        /// <summary>
        /// Go Back Command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async Task GoBack()
        {
            _logService.LogMethodEntry();
            try
            {
                RaiseExceptionForTests();
                await _navigationService!.NavigateBack();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
            }
            _logService.LogMethodExit();
        }

        public override async Task OnAppearing()
        {
            _logService.LogMethodEntry();
            await base.OnAppearing();
            BusNumber = _appConfigurationsService?.AppSessionDataObj?.BusObj?.BusNumber;
            SelectedRoute = _appConfigurationsService?.AppSessionDataObj?.TripRouteObj?.RouteName;
            StartMiles = _appConfigurationsService?.AppSessionDataObj?.StartMiles.ToString();
            DrivenMiles = EndMiles = _appConfigurationsService!.DrivenMiles.ToString("0");
            _logService.LogMethodExit();
        }

        #endregion
    }
}
