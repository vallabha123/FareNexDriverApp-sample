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
using FareNexDriverApp.Core.Interfaces.Rest;
using FareNexDriverApp.Core.Utilities;

namespace FareNexDriverApp.Rest.Services
{
    public class SseEventsApiService(HttpClient httpClient,
        IAppConfigurationsService appConfigurationsService,
        ILocalizationService localizationService,
        ILogService<SseEventsApiService> logService,
        IConnectivityService connectivityService) : BaseApiService(httpClient, appConfigurationsService, localizationService), ISseEventsApiService
    {
        readonly ILogService<SseEventsApiService> _logService = logService;
        readonly IConnectivityService _connectivityService = connectivityService;
        public CancellationTokenSource? CancellationToken { get; set; }
        private const int MaxReconnectAttempts = 5; // Maximum number of reconnect attempts
        private const int Reconnect5SecsDelayMs = 5000; // Delay (ms) before retrying connection
        private const int Reconnect5MinsDelayMs = 5 * 60 * 1000; // Delay (ms) before retrying connection
        private int _reconnectAttempts;
        public bool IsSseEventsListenerRunning { get; set; }

        /// <summary>
        /// Event triggered when an SSE message is received.
        /// </summary>
        public event Action<NotificationType, object>? OnMessageReceived;

        Task? BGWorkerTask;

        /// <summary>
        /// Event triggered when the SSE connection is lost.
        /// </summary>
        public event Action<int>? OnConnectionLost;

        /// <summary>
        /// Event triggered when the SSE connection is successfully re-established.
        /// </summary>
        public event Action? OnReconnected;

        public void StartSseEventsStreamListenerAsync()
        {
            _logService.LogMethodEntry();
            BGWorkerTask = Task.Run(async () =>
            {
                try
                {
                    _logService.LogInformation("Connecting to SSE events service...");
                    IsSseEventsListenerRunning = true;
                    string requestUri = string.Format(GetRequestUri(ConfigurationKeys.SseEvents), _appConfigurationsService.AndroidGsfId);
                    _logService.LogInformation($"Rest call - {requestUri}");
                    using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    await AttachHeadersToHttpClient();

                    using var response = await _httpClient!.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken!.Token);
                    response.EnsureSuccessStatusCode();

                    _logService.LogInformation("Connected to SSE server.");
                    _reconnectAttempts = 0;
                    OnReconnected?.Invoke();

                    using var stream = await response.Content.ReadAsStreamAsync(CancellationToken.Token);
                    using var reader = new StreamReader(stream);

                    while (!reader.EndOfStream && !CancellationToken.Token.IsCancellationRequested)
                    {
                        var line = await reader.ReadLineAsync(CancellationToken.Token);
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith(':'))
                            continue;

                        HandleSseEventStreamReaderLines(line);
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logService.LogException(ex);
                    PerformSseRetry();
                }
                catch (TaskCanceledException ex)
                {
                    _logService.LogException(ex);
                    PerformSseRetry();
                }
                catch (IOException ex)
                {
                    _logService.LogException(new Exception(ex.Message));
                    PerformSseRetry();
                }
                catch (WebException ex)
                {
                    _logService.LogException(new Exception(ex.Message));
                    PerformSseRetry();
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex);
                    PerformSseRetry();
                }
            }, CancellationToken!.Token);
            _logService.LogMethodExit();
        }

        void HandleSseEventStreamReaderLines(string line)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    if (line.StartsWith("event:"))
                    {
                        HandleEventLineFromStream(line);
                    }
                    else if (line.StartsWith("data:"))
                    {
                        HandleDataLineFromStream(line);
                    }
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex);
                }
            });
        }

        void HandleEventLineFromStream(string line)
        {
            _logService.LogInformation($"Received SSE message: - event - {line}");
            if (line.Contains("close"))
            {
                StopSseEventsListener();
            }
        }

        void HandleDataLineFromStream(string line)
        {
            _logService.LogInformation($"Received SSE message: - data - {line}");
            if (line.Contains("data: {}"))
                return;

            NotificationType _notificationType = NotificationType.None;
            line = line["data:".Length..].Trim();
            if (line.StartsWith("{\"Event\":0,"))
            {
                _notificationType = NotificationType.Payment;
            }
            else if (line.StartsWith("{\"Event\":2,"))
            {
                _notificationType = NotificationType.PassengerInOut;
            }
            else if (line.StartsWith("PaymentHistory"))
            {
                _notificationType = NotificationType.PaymentHistory;
                string[] strArr = line.Split('|');
                line = strArr[1].Trim();
            }
            OnMessageReceived?.Invoke(_notificationType, line);
        }

        /// <summary>
        /// Stops listening for SSE messages.
        /// </summary>
        public async void StopSseEventsListener()
        {
            _logService.LogMethodEntry("Stopping SSE listener...");
            if (CancellationToken is not null)
            {
                await CancellationToken.CancelAsync();
                OnMessageReceived = null;
                OnConnectionLost = null;
                OnReconnected = null;
                IsSseEventsListenerRunning = false;
            }
            _logService.LogMethodExit("SSE listener stopped - " + BGWorkerTask?.IsCompleted + BGWorkerTask?.IsCanceled);
        }

        void PerformSseRetry()
        {
            Task.Run(async () =>
            {
                try
                {
                    _logService.LogMethodEntry("Sse Retry Attempt No - " + _reconnectAttempts);
                    bool isSkipWait = false;
                    while (!_connectivityService.IsNetworkAvailable())
                    {
                        await Task.Delay(1000);
                        isSkipWait = true;
                    }
                    if (_reconnectAttempts > MaxReconnectAttempts)
                    {
                        OnConnectionLost?.Invoke(_reconnectAttempts);
                    }
                    if (!isSkipWait)
                    {
                        await Task.Delay(_reconnectAttempts <= MaxReconnectAttempts ? Reconnect5SecsDelayMs : Reconnect5MinsDelayMs, CancellationToken!.Token);
                    }
                    _reconnectAttempts++;
                    StartSseEventsStreamListenerAsync();
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex);
                }
            }, CancellationToken!.Token);
        }
    }
}