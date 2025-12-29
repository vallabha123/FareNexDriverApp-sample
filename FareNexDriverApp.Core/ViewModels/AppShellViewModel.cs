using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.ViewModels.BaseViewModels;
using System;

namespace FareNexDriverApp.Core.ViewModels
{
    public class AppShellViewModel : BaseNavigationViewModel
    {
        public AppShellViewModel(IServiceProvider serviceProvider,
            IPopupService popupService,
            IConnectivityService connectivityService,
            ILogService<AppShellViewModel> logService) : base(serviceProvider, popupService, connectivityService)
        {
            _connectionService = connectivityService;
            _connectionService?.StartConnectivityListener();
            logService.LogMethodExit();
        }
    }
}