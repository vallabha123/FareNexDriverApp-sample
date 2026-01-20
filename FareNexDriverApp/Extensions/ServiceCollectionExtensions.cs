using FareNexDriverApp.Core.ViewModels.BaseViewModels;
using FareNexDriverApp.Services;

namespace FareNexDriverApp.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSingletonPageWithShellRoute<TPage, TViewModel>(
            this IServiceCollection services)
            where TPage : Page
            where TViewModel : BaseViewModel
        {
            NavigationService.AddSingletonPageWithShellRoute<TPage, TViewModel>(services);
            return services;
        }

        public static IServiceCollection AddTransientPageWithShellRoute<TPage, TViewModel>(
            this IServiceCollection services)
            where TPage : Page
            where TViewModel : BaseViewModel
        {
            NavigationService.AddTransientPageWithShellRoute<TPage, TViewModel>(services);
            return services;
        }
    }
}
