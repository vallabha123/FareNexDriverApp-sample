using System.Threading.Tasks;

namespace FareNexDriverApp.Core.Interfaces.Core; public interface IPopupService{ Task ShowLoadingAsync(); Task CloseLoadingAsync(); Task ShowPopupAsync(object o); }