using FareNexDriverApp.Core.Models.CustomModels;
using System.Threading.Tasks;

namespace FareNexDriverApp.Core.Interfaces.Core
{
    public interface IPopupService
    {
        Task ShowPopupAsync(AlertPopupModel alertPopupModel);

        Task ShowLoadingAsync();

        Task ClosePopupAsync(object? result = null);

        Task CloseLoadingAsync();
    }
}