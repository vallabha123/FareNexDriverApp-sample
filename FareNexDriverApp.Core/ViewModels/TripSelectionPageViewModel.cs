using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FareNexDriverApp.Core.EventArgs;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Configurations;
using FareNexDriverApp.Core.Interfaces.Core.Localizations;
using FareNexDriverApp.Core.Interfaces.Rest;
using FareNexDriverApp.Core.Models.CustomModels;
using FareNexDriverApp.Core.Models.DriverLogin;
using FareNexDriverApp.Core.Utilities;
using FareNexDriverApp.Core.ViewModels.BaseViewModels;
using System;
using System.Reflection;
using System.Threading.Tasks;


namespace FareNexDriverApp.Core.ViewModels
{
    public partial class TripSelectionPageViewModel : BaseNavigationViewModel
    {
        private readonly ILogService<TripSelectionPageViewModel> _logService;

        #region Properties

        [ObservableProperty]
        private bool _isTripSelectionPageVisible = true;

        [ObservableProperty]
        private bool _isChangeSelectionPageVisible;

        [ObservableProperty]
        private CustomDropdownModel? _routeBindingContext;

        [ObservableProperty]
        private string? _startMiles;

        [ObservableProperty]
        private string? _busNumber;

        [ObservableProperty]
        private string? _driverName;

        [ObservableProperty]
        private string? _routeSelectedValue;

        [ObservableProperty]
        private bool _isDropDownVisible = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logService"></param>
        public TripSelectionPageViewModel(IServiceProvider serviceProvider,
            IPopupService popupService,
            ILogService<TripSelectionPageViewModel> logService,
            INavigationService navigationService,
            ITripSelectionApiService tripSelectionApiService,
            IAppConfigurationsService appConfigurationsService,
            IConnectivityService connectivityService,
            IEndTripApiService endTripApiService,
            ILocalizationService localizationService) : base(serviceProvider, popupService, connectivityService)
        {
            _logService = logService;
            _navigationService = navigationService;
            _tripSelectionApiService = tripSelectionApiService;
            _appConfigurationsService = appConfigurationsService;
            _connectionService = connectivityService;
            _endTripApiService = endTripApiService;
            _localizationService = localizationService;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Next Button Command
        /// </summary>
        [RelayCommand]
        public void Next()
        {
            _logService.LogMethodEntry();
            try
            {
                RaiseExceptionForTests();
                RouteSelectedValue = RouteBindingContext?.SelectedRoute?.RouteName;
                IsTripSelectionPageVisible = false;
                IsChangeSelectionPageVisible = true;
                WeakReferenceMessenger.Default.Send(new AnimateButtonMessage(AppConstants.AnimationMessageStart));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
            }
            _logService.LogMethodExit();
        }

        /// <summary>
        /// Re EnterPin  Button
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async Task PinChange()
        {
            if (!IsLoadingIndicatorRunning && IsNetworkAvailable)
            {
                await ShowLoadingIndicator();
                _logService.LogMethodEntry();
                try
                {
                    RaiseExceptionForTests();
                    Tuple<bool, ErrorModel?> resTuple = await _endTripApiService!.EndTripApi(0);
                    if (resTuple is not null
                        && resTuple.Item1)
                    {
                        await _appConfigurationsService!.ClearAndResetCacheData();
                        await Task.Delay(50);
                        await _navigationService!.NavigateBack();
                    }
                    else
                    {
                        await HandleApiErrorResponse(resTuple);
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
        /// Change Route Command
        /// </summary>
        [RelayCommand]
        public void ChangeRoute()
        {
            _logService.LogMethodEntry();
            if (!IsLoadingIndicatorRunning)
            {
                try
                {
                    RaiseExceptionForTests();
                    IsChangeSelectionPageVisible = false;
                    IsTripSelectionPageVisible = true;
                    WeakReferenceMessenger.Default.Send(new AnimateButtonMessage(AppConstants.AnimationMessageCancel));
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex);
                }
                _logService.LogMethodExit();
            }
        }

        /// <summary>
        /// Start Ride Command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async Task StartRideButton(object isCancel)
        {
            if (!IsLoadingIndicatorRunning)
            {
                await ShowLoadingIndicator();
                _logService.LogMethodEntry();
                try
                {
                    RaiseExceptionForTests();
                    if (Convert.ToBoolean(isCancel))
                    {
                        WeakReferenceMessenger.Default.Send(new AnimateButtonMessage(AppConstants.AnimationMessageCancel));
                    }
                    if (IsNetworkAvailable)
                    {
                        string? selectedRouteId = RouteBindingContext?.SelectedRoute?.RouteID;
                        string? selectedRouteFareCategory = RouteBindingContext?.SelectedRoute?.FareCategory;
                        if (!string.IsNullOrEmpty(selectedRouteId))
                        {
                            Tuple<bool, ErrorModel?> resTuple = await _tripSelectionApiService!.StartTripApi(selectedRouteId, selectedRouteFareCategory);
                            if (resTuple is not null
                                && resTuple.Item1)
                            {
                                _appConfigurationsService!.AppSessionDataObj.TripRouteObj = RouteBindingContext?.SelectedRoute;
                                WeakReferenceMessenger.Default.Send(new AnimateButtonMessage(AppConstants.AnimationMessageCancelAndUnregister));
                                await Task.Delay(50);
                                await _navigationService!.NavigateTo<HomePageViewModel>();
                            }
                            else
                            {
                                await HandleApiErrorResponse(resTuple);
                            }
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

        private async Task HandleApiErrorResponse(Tuple<bool, ErrorModel?>? resTuple)
        {
            string errorContent = resTuple?.Item2?.ErrorMessage!;
            string title = resTuple?.Item2?.ErrorTitle!;
            await HideLoadingIndicator();
            await _popupService!.ShowPopupAsync(new Models.CustomModels.AlertPopupModel(title, errorContent));
            if (title == _localizationService?.GetLocalizedValue("SessionExpired"))
            {
                await _navigationService!.NavigateBack();
            }
        }

        public override async Task OnAppearing()
        {
            await base.OnAppearing();
            _ = LoadRoute();
        }

        public override Task OnDisappearing()
        {
            base.OnDisappearing();
            WeakReferenceMessenger.Default.Send(new AnimateButtonMessage(AppConstants.AnimationMessageCancelAndUnregister));
            return Task.CompletedTask;
        }

        public async Task LoadRoute()
        {
            await Task.Delay(100);
            await Task.Run(() =>
            {
                _logService.LogMethodEntry();
                try
                {
                    RaiseExceptionForTests();
                    if (_appConfigurationsService?.AppSessionDataObj is not null)
                    {
                        StartMiles = _appConfigurationsService.AppSessionDataObj.StartMiles.ToString("0");
                        BusNumber = _appConfigurationsService.AppSessionDataObj.BusObj?.BusNumber;
                        DriverName = _appConfigurationsService.AppSessionDataObj.DriveInfoObj?.DriverName;
                        if (_appConfigurationsService.AppSessionDataObj.RouteList?.Count > 0)
                        {
                            RouteBindingContext = new()
                            {
                                DropdownList = [.. _appConfigurationsService.AppSessionDataObj.RouteList],
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex);
                }
                _logService.LogMethodExit();
            });
        }

        /// <summary>
        /// Select Route Tap GesterCommand
        /// </summary>
        /// <param name="selected"></param>
        [RelayCommand]
        public void SelectRoute(RouteModel selected)
        {
            if (selected == null)
                return;

            foreach (var route in RouteBindingContext!.DropdownList!.Where(s => s.IsSelected))
            {
                route.IsSelected = false;
            }

            selected.IsSelected = true;
            RouteBindingContext.DropdownTitle = selected.RouteName!;
            RouteBindingContext.SelectedRoute = selected;
            RouteBindingContext.IsDropDownVisible = false;
            RouteBindingContext.IsSelected = true;
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
