using CommunityToolkit.Mvvm.ComponentModel;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Configurations;
using FareNexDriverApp.Core.Interfaces.Core.Localizations;
using FareNexDriverApp.Core.Interfaces.Rest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FareNexDriverApp.Core.ViewModels.BaseViewModels
{
    public partial class BaseNavigationViewModel(IServiceProvider serviceProvider,
        IPopupService popupService,
        IConnectivityService connectivityService) : BaseViewModel
    {
        protected readonly IServiceProvider _serviceProvider = serviceProvider;
        protected INavigationService? _navigationService;
        protected IAppConfigurationsService? _appConfigurationsService;
        protected IConnectivityService? _connectionService = connectivityService;
        protected ILocalizationService? _localizationService;
        protected ITripSelectionApiService? _tripSelectionApiService;
        protected IDriverLoginApiService? _driverLoginApiService;
        protected IEndTripApiService? _endTripApiService;
        protected readonly IPopupService? _popupService = popupService;
        protected ISseEventsApiService? _sseEventsApiService;
        protected ILogUploadApiService? _logUploadApiService;

        public bool RaiseExceptionForTest { get; set; }

        public override Task OnAppearing()
        {
            _connectionService!.OnConnectionStatusChanged -= ConnectionService_OnConnectionStatusChanged;
            _connectionService.OnConnectionStatusChanged += ConnectionService_OnConnectionStatusChanged;
            ConnectionService_OnConnectionStatusChanged(this, _connectionService.IsNetworkAvailable());
            return base.OnAppearing();
        }

        public override Task OnDisappearing()
        {
            _connectionService!.OnConnectionStatusChanged -= ConnectionService_OnConnectionStatusChanged;
            return base.OnDisappearing();
        }

        private async void ConnectionService_OnConnectionStatusChanged(object? sender, bool e)
        {
            IsNetworkAvailable = e;
            if (IsNetworkAvailable && IsNetworkToastVisible)
            {
                await Task.Delay(3000);
                IsNetworkToastVisible = false;
            }
            else if (!IsNetworkAvailable)
            {
                IsNetworkToastVisible = true;
            }
        }

        protected void RaiseExceptionForTests()
        {
#if DEBUG || QA
            // Sample Exception raised to cover catch blocks for unit tests
            if (RaiseExceptionForTest)
            {
                throw new InvalidOperationException("Raised exception for unit test");
            }
#endif
        }

        [ObservableProperty]
        private bool _isNetworkAvailable;

        [ObservableProperty]
        private bool _isNetworkToastVisible;

        protected async Task ShowLoadingIndicator()
        {
            await _popupService!.ShowLoadingAsync()!;
            _isLoading = true;
        }

        protected async Task HideLoadingIndicator()
        {
            await _popupService!.CloseLoadingAsync();
            _isLoading = false;
        }

        protected bool IsLoadingIndicatorRunning => _isLoading;

    }
}

