using System.Collections.Generic;
using System.Threading.Tasks;

namespace FareNexDriverApp.Core.Interfaces.Core
{
    public interface INavigationService
    {
        Task NavigateTo<T>(
            IDictionary<string, object>? parameters = null,
            bool animate = true);

        Task NavigateToRoute<T>(
            IDictionary<string, object>? parameters = null,
            bool animate = true);

        Task NavigateToRoute<T1, T2>(
            IDictionary<string, object>? parameters = null,
            bool animate = true);

        Task NavigateBack(
            IDictionary<string, object>? parameters = null,
            bool animate = true);

        void NavigateToMainPage(
            IDictionary<string, object>? parameters = null,
            bool animate = true);
    }
}
