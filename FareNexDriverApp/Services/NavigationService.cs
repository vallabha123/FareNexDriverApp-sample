using CommunityToolkit.Maui;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.ViewModels;
using FareNexDriverApp.Core.ViewModels.BaseViewModels;

namespace FareNexDriverApp.Services
{
    public class NavigationService : INavigationService
    {
        private static readonly Dictionary<Type, Type> _pageLookUp = [];

        private readonly ILogService<NavigationService> _logService;
        private readonly AppShellViewModel _appShellViewModel;

        public NavigationService(
            ILogService<NavigationService> logService,
            AppShellViewModel appShellViewModel)
        {
            _logService = logService;
            _appShellViewModel = appShellViewModel;
        }

        #region Page Registration

        public static void AddSingletonPageWithShellRoute<TPage, TViewModel>(
            IServiceCollection services)
            where TPage : Page
            where TViewModel : BaseViewModel
        {
            _pageLookUp.Add(typeof(TViewModel), typeof(TPage));

            services.AddSingletonWithShellRoute<TPage, TViewModel>(typeof(TPage).Name);
        }

        public static void AddTransientPageWithShellRoute<TPage, TViewModel>(
            IServiceCollection services)
            where TPage : Page
            where TViewModel : BaseViewModel
        {
            _pageLookUp.Add(typeof(TViewModel), typeof(TPage));

            services.AddTransientWithShellRoute<TPage, TViewModel>(typeof(TPage).Name);
        }

        #endregion

        #region Navigation

        async Task INavigationService.NavigateBack(
            IDictionary<string, object>? parameters,
            bool animate)
        {
            try
            {
                await Shell.Current.GoToAsync(
                    "..",
                    animate,
                    parameters ?? new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
            }
        }

        async Task INavigationService.NavigateTo<T>(
            IDictionary<string, object>? parameters,
            bool animate)
        {
            try
            {
                await Shell.Current.GoToAsync(
                    LookUpPageFromViewModel<T>(),
                    animate,
                    parameters ?? new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
            }
        }

        async Task INavigationService.NavigateToRoute<T>(
            IDictionary<string, object>? parameters,
            bool animate)
        {
            try
            {
                await Shell.Current.GoToAsync(
                    $"//{LookUpPageFromViewModel<T>()}",
                    animate,
                    parameters ?? new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
            }
        }

        async Task INavigationService.NavigateToRoute<T1, T2>(
            IDictionary<string, object>? parameters,
            bool animate)
        {
            try
            {
                await Shell.Current.GoToAsync(
                    $"//{LookUpPageFromViewModel<T1>()}/{LookUpPageFromViewModel<T2>()}",
                    animate,
                    parameters ?? new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
            }
        }

        void INavigationService.NavigateToMainPage(
            IDictionary<string, object>? parameters,
            bool animate)
        {
            try
            {
                Application.Current!.Windows[0].Page =
                    new AppShell(_appShellViewModel);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
            }
        }

        #endregion

        #region Helpers

        private static string LookUpPageFromViewModel<T>()
        {
            _pageLookUp.TryGetValue(typeof(T), out var page);
            return page!.Name;
        }

        #endregion
    }
}
