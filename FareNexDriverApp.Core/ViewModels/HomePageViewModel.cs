using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FareNexDriverApp.Core.EventArgs;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Configurations;
using FareNexDriverApp.Core.Interfaces.Rest;
using FareNexDriverApp.Core.Models.Passenger;
using FareNexDriverApp.Core.Models.Payments;
using FareNexDriverApp.Core.Services;
using FareNexDriverApp.Core.Utilities;
using FareNexDriverApp.Core.ViewModels.BaseViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FareNexDriverApp.Core.ViewModels
{
    public partial class HomePageViewModel : BaseNavigationViewModel, IDisposable
    {
        #region Properties

        private readonly ILogService<HomePageViewModel> _logger;
        private readonly ILocationService _locationService;

        [ObservableProperty]
        private PassengerModel? _passengerModels = new();

        [ObservableProperty]
        private ObservableCollection<PaymentsModel>? _paymentsList = [];

        [ObservableProperty]
        private string? _destinationLocationName;

        [ObservableProperty]
        private string? _originLocationName;

        [ObservableProperty]
        private int _totalPassengersCount;

        [ObservableProperty]
        private int _totalTicketsTransacted;

        [ObservableProperty]
        private string _currentDateTime = DateTime.Now.ConvertDateTimeToString(AppConstants.DateTimeDashboardFormat);

        private CancellationTokenSource? _dateTimeCancellationTokenSource;

        [ObservableProperty]
        private string? _selectedRouteName;

        [ObservableProperty]
        private bool _isLocalFare;

        [ObservableProperty]
        private bool _isSseConnectionUnavailable;

        [ObservableProperty]
        private bool _isFrontBackHeadersVisible;

        [ObservableProperty]
        private int _totalFrontTickets;

        [ObservableProperty]
        private int _totalBackTickets;

        #endregion

        #region Constructor

        public HomePageViewModel(IServiceProvider serviceProvider,
             IPopupService popupService,
             ILogService<HomePageViewModel> logService,
             IAppConfigurationsService appConfigurationsService,
             INavigationService navigationService,
             IConnectivityService connectivityService,
             ILocationService locationService,
             ISseEventsApiService sseEventsApiService) : base(serviceProvider, popupService, connectivityService)
        {
            _logger = logService;
            _appConfigurationsService = appConfigurationsService;
            _navigationService = navigationService;
            _connectionService = connectivityService;
            _locationService = locationService;
            _sseEventsApiService = sseEventsApiService;
            _sseEventsApiService.IsSseEventsListenerRunning = false;
            _logger.LogInformation("Constructor intialized");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Start date time dispatcher
        /// </summary>
        private async Task StartDateTimeDispatcher()
        {
            _dateTimeCancellationTokenSource = new();
            await Task.Run(async () =>
            {
                _logger.LogMethodEntry();
                try
                {
                    int delayInMillis = 1000;
                    DateTime prevDateTime = DateTime.Now;
                    while (true)
                    {
                        if (DateTime.Now.Minute != prevDateTime.Minute)
                        {
                            CurrentDateTime = DateTime.Now.ConvertDateTimeToString(AppConstants.DateTimeDashboardFormat);
                            delayInMillis = 60000;
                            prevDateTime = DateTime.Now;
                        }
                        await Task.Delay(delayInMillis);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
                _logger.LogMethodExit();
            }, _dateTimeCancellationTokenSource.Token);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// OnAppearing called when page is displaying
        /// </summary>
        public override async Task OnAppearing()
        {
            await base.OnAppearing();
            _ = LoadAsync();
        }

        /// <summary>
        /// OnDisappearing raised when leaving page
        /// </summary>
        public override async Task OnDisappearing()
        {
            _logger.LogMethodEntry();
            await base.OnDisappearing();
            Dispose(true);
            _logger.LogMethodExit();
        }

        private async Task LoadAsync()
        {
            await Task.Delay(100);
            await Task.Run(() =>
            {
                _logger.LogMethodEntry();
                try
                {
                    RaiseExceptionForTests();
                    _ = StartDateTimeDispatcher();
                    SelectedRouteName = _appConfigurationsService?.AppSessionDataObj?.TripRouteObj?.RouteName;
                    IsLocalFare = _appConfigurationsService?.AppSessionDataObj?.TripRouteObj?.FareCategory == AppConstants.LocalFareCategory;
                    IsFrontBackHeadersVisible = _appConfigurationsService!.AppSessionDataObj.IsMultiDoorBus;
                    if (!_sseEventsApiService!.IsSseEventsListenerRunning)
                    {
                        _sseEventsApiService.CancellationToken = new();
                        _sseEventsApiService.OnMessageReceived -= SseEventsApiService_OnMessageReceived;
                        _sseEventsApiService.OnMessageReceived += SseEventsApiService_OnMessageReceived;
                        _sseEventsApiService.OnConnectionLost -= SseEventsApiService_OnConnectionLost;
                        _sseEventsApiService.OnConnectionLost += SseEventsApiService_OnConnectionLost;
                        _sseEventsApiService.OnReconnected -= SseEventsApiService_OnReconnected;
                        _sseEventsApiService.OnReconnected += SseEventsApiService_OnReconnected;
                        _sseEventsApiService.StartSseEventsStreamListenerAsync();
                    }
                    StartCapturingLocationBreadCrumpsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
                _logger.LogMethodExit();
            });
        }

        private void SseEventsApiService_OnReconnected()
        {
            IsSseConnectionUnavailable = false;
        }

        private void SseEventsApiService_OnConnectionLost(int retryCount)
        {
            if (retryCount > 5)
            {
                Task.Run(async () =>
                {
                    IsSseConnectionUnavailable = true;
                    await Task.Delay(4000);
                    IsSseConnectionUnavailable = false;
                });
            }
        }

        private void SseEventsApiService_OnMessageReceived(NotificationType notificationType, object messageModel)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("SSE OnMessage received event fired");
                    if (messageModel is not null)
                    {
                        switch (notificationType)
                        {
                            case NotificationType.Payment:
                                ProcessPaymentStreamMessage(messageModel.ToString());
                                break;
                            case NotificationType.PassengerInOut:
                                ProcessPassengerStreamMessage(messageModel.ToString());
                                break;
                            case NotificationType.PaymentHistory:
                                ProcessPaymentsListStreamMessage(messageModel.ToString());
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
            });
        }

        private void ProcessPaymentsListStreamMessage(string? paymentListJson)
        {
            _logger.LogMethodEntry();
            List<PaymentNotification>? paymentNotificationList = JsonSerializer.Deserialize<List<PaymentNotification>>(paymentListJson!);
            if (paymentNotificationList?.Count > 0)
            {
                PaymentsList = [];
                foreach (var item in paymentNotificationList)
                {
                    AddTransactionItemToCollection(item);
                }
                StrongReferenceMessenger.Default.Send(new ItemUpdateScrollMessage(string.Empty));
            }

            _logger.LogMethodExit();
        }

        private void ProcessPassengerStreamMessage(string? passengerInOutNotificationJson)
        {
            _logger.LogMethodEntry();
            PassengerInOutNotification? passengerInOutNotification = JsonSerializer.Deserialize<PassengerInOutNotification>(passengerInOutNotificationJson!);
            if (PassengerModels is not null && passengerInOutNotification is not null)
            {
                TotalPassengersCount = passengerInOutNotification.TotalPassengersBoarded;
                PassengerModels.TotalPassengers.TotalAdultCount = passengerInOutNotification.AdultsOnboard;
                PassengerModels.TotalPassengers.TotalChildrenCount = passengerInOutNotification.ChildrenOnboard;
                PassengerModels.TotalPassengers.TotalCount = passengerInOutNotification.PassengersOnboard;

                PassengerModels.LastEntryCounts.TotalAdultCount = passengerInOutNotification.LastStopEntryAdults;
                PassengerModels.LastEntryCounts.TotalChildrenCount = passengerInOutNotification.LastStopEntryChildren;
                PassengerModels.LastEntryCounts.TotalCount = passengerInOutNotification.LastStopEntry;

                PassengerModels.LastExitCounts.TotalAdultCount = passengerInOutNotification.LastStopExitAdults;
                PassengerModels.LastExitCounts.TotalChildrenCount = passengerInOutNotification.LastStopExitChildren;
                PassengerModels.LastExitCounts.TotalCount = passengerInOutNotification.LastStopExit;
            }
            _logger.LogMethodExit();
        }

        private void ProcessPaymentStreamMessage(string? paymentJson)
        {
            _logger.LogMethodEntry();
            PaymentNotification? paymentNotification = JsonSerializer.Deserialize<PaymentNotification>(paymentJson!);
            if (paymentNotification is null)
                return;

            AddTransactionItemToCollection(paymentNotification);
            StrongReferenceMessenger.Default.Send(new ItemUpdateScrollMessage(string.Empty));
            if (PaymentsList?.Count > 10)
            {
                PaymentsList.RemoveAt(10);
            }
            _logger.LogMethodExit();
        }

        private void AddTransactionItemToCollection(PaymentNotification paymentNotification)
        {
            TotalTicketsTransacted = paymentNotification.TotalTicketsTransacted;
            TotalFrontTickets = paymentNotification.TotalFrontTicketsTransacted;
            TotalBackTickets = paymentNotification.TotalBackTicketsTransacted;
            var paymentModel = new PaymentsModel()
            {
                TotalAmount = paymentNotification.Amount.ToString("F2"),
                IsTransactionSuccess = paymentNotification.Status == "Success",
                TransactionDate = paymentNotification.TransactionTime.ToString("hh:mm tt"),
                GridColumnDefinitions = []
            };
            if (IsFrontBackHeadersVisible)
            {
                paymentModel.IsFrontDoorTransaction = paymentNotification.BusDoorType == DoorType.Front;
                paymentModel.IsBackDoorTransaction = paymentNotification.BusDoorType == DoorType.Back;
            }
            int ColumnCount = 0;
            foreach (var item in paymentNotification.TicketCounts.OrderBy(o => o.Key))
            {
                switch (item.Key)
                {
                    case PassengerType.Adults:
                        paymentModel.IsAdultTransactionViewVisible = true;
                        paymentModel.TotalGeneralTickets = item.Value;
                        paymentModel.AdultCardColumnNumber = ColumnCount++;
                        paymentModel = AddGridColumnDefinition(paymentModel, paymentNotification.TicketCounts.Count);
                        break;
                    case PassengerType.Under13:
                        paymentModel.IsYouthTransactionViewVisible = true;
                        paymentModel.TotalYouthTickets = item.Value;
                        paymentModel.Under13CardColumnNumber = ColumnCount++;
                        paymentModel = AddGridColumnDefinition(paymentModel, paymentNotification.TicketCounts.Count);
                        break;
                    case PassengerType.Under3:
                        paymentModel.IsChildTransactionViewVisible = true;
                        paymentModel.TotalChildTickets = item.Value;
                        paymentModel.Under3CardColumnNumber = ColumnCount++;
                        paymentModel = AddGridColumnDefinition(paymentModel, paymentNotification.TicketCounts.Count);
                        break;
                    case PassengerType.Over65_ADA:
                        paymentModel.IsSeniorTransactionViewVisible = true;
                        paymentModel.TotalSeniorCitizenTickets = item.Value;
                        paymentModel.Over64ADACardColumnNumber = ColumnCount;
                        paymentModel = AddGridColumnDefinition(paymentModel, paymentNotification.TicketCounts.Count, 31);
                        break;
                    default:
                        break;
                }
            }
            PaymentsList!.Insert(0, paymentModel);
        }

        private static PaymentsModel AddGridColumnDefinition(PaymentsModel paymentModel, int count, double gridLengthValue = 23)
        {
            if (count == 4)
            {
                paymentModel.GridColumnDefinitions!.Add(new ColumnDefinition(new GridLength(gridLengthValue, GridUnitType.Star)));
            }
            else
            {
                paymentModel.GridColumnDefinitions!.Add(new ColumnDefinition(GridLength.Star));
            }
            return paymentModel;
        }

        /// <summary>
        /// Goto End Ride Command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async Task GotoEndRide()
        {
            _logger.LogMethodEntry();
            await ShowLoadingIndicator();
            try
            {
                RaiseExceptionForTests();
                await Task.Delay(50);
                await _navigationService!.NavigateTo<EndTripPageViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            finally
            {
                await HideLoadingIndicator();
            }
            _logger.LogMethodExit();

        }

        /// <summary>
        /// IDisposable.Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Perform cleanups
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            _logger.LogMethodEntry();
            try
            {
                _dateTimeCancellationTokenSource?.Cancel();
                _dateTimeCancellationTokenSource?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            _logger.LogMethodExit();
        }

        /// <summary>
        /// Starts background task for capturing location updates
        /// </summary>
        /// <returns></returns>
        public void StartCapturingLocationBreadCrumpsAsync()
        {
            if (!_locationService.IsListeningForeground)
            {
                _ = GeoLocationMonitor
                    .Instance
                    .StartCapturingLocationBreadCrumpsAsync(
                            _serviceProvider.GetService<ILogService<GeoLocationMonitor>>()!
                            , _locationService
                            , _appConfigurationsService!);
            }
        }

        #endregion
    }
}
