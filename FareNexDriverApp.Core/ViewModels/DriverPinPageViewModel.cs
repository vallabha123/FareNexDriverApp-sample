using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Configurations;
using FareNexDriverApp.Core.Interfaces.Rest;
using FareNexDriverApp.Core.Models.DriverLogin;
using FareNexDriverApp.Core.ViewModels.BaseViewModels;
using System;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FareNexDriverApp.Core.ViewModels
{
    public partial class DriverPinPageViewModel : BaseNavigationViewModel
    {

        #region Properties

        private readonly ILogService<DriverPinPageViewModel> _logService;

        [ObservableProperty]
        private string? _pin;

        [ObservableProperty]
        private bool _isError;

        [ObservableProperty]
        private string? _message;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logService"></param>
        public DriverPinPageViewModel(IServiceProvider serviceProvider,
            IPopupService popupService,
            ILogService<DriverPinPageViewModel> logService,
            INavigationService navigationService,
            IAppConfigurationsService appConfigurationsService,
            IDriverLoginApiService driverLoginApiService,
            IConnectivityService connectivityService) : base(serviceProvider, popupService, connectivityService)
        {
            _logService = logService;
            _navigationService = navigationService;
            _appConfigurationsService = appConfigurationsService;
            _driverLoginApiService = driverLoginApiService;
            _connectionService = connectivityService;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Login Command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async Task Login()
        {
            IsError = false;
            Message = string.Empty;
            if (!IsLoadingIndicatorRunning && IsNetworkAvailable)
            {
                _logService.LogMethodEntry();
                await ShowLoadingIndicator();
                try
                {
                    if (!string.IsNullOrWhiteSpace(Pin) || Pin?.Length < 4)
                    {

                        RaiseExceptionForTests();
                        Tuple<bool, AppSessionData?> tuple = await _driverLoginApiService!.LoginApi(Pin);
                        if (tuple is not null
                            && tuple.Item1
                            && tuple.Item2 is not null)
                        {
                            _appConfigurationsService!.AppSessionDataObj = tuple.Item2;
                            await _navigationService!.NavigateTo<TripSelectionPageViewModel>();
                            Pin = string.Empty;
                        }
                        else
                        {
                            IsError = true;
                            string errorContent = tuple?.Item2?.ErrorModel?.ErrorMessage!;
                            if (tuple?.Item2?.ErrorModel?.ErrorDisplayType == Utilities.ErrorDisplayType.Inline)
                            {
                                Message = errorContent;
                            }
                            else
                            {
                                string title = tuple?.Item2?.ErrorModel?.ErrorTitle!;
                                await HideLoadingIndicator();
                                await _popupService!.ShowPopupAsync(new Models.CustomModels.AlertPopupModel(title, errorContent));
                            }
                            Pin = string.Empty;
                        }
                    }
                    else
                    {
                        IsError = true;
                        Message = "Your PIN must be 4 digits long";
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

        #endregion

        #region Public Methods

        [RelayCommand]
        public void PinTextChanged()
        {
            if (Pin?.Length > 0 && !PinValidationRegex().IsMatch(Pin))
            {
                Pin = Pin.Substring(0, Pin.Length - 1);
            }
        }

        [GeneratedRegex(@"^\d+$")]
        private partial Regex PinValidationRegex();

        public override async Task OnAppearing()
        {
            await base.OnAppearing();
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (await Permissions.RequestAsync<Permissions.LocationWhenInUse>() == PermissionStatus.Granted)
                {
                    await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
            });
        }


        #endregion
    }
}
