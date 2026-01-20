using CommunityToolkit.Maui.Views;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Utilities;
using FareNexDriverApp.Core.Models.CustomModels;
using FareNexDriverApp.Core.Utilities;
using FareNexDriverApp.Views;
using FareNexDriverApp.Views.PopupViews;

namespace FareNexDriverApp.Services
{
    public class PopupService : IPopupService
    {
        private readonly List<BasePopup> _currentPopups = [];

        public async Task ClosePopupAsync(object? result = null)
        {
            if (_currentPopups?.Count > 0)
            {
                await _currentPopups[0].CloseAsync(result);
            }
        }

        public async Task CloseLoadingAsync()
        {
            if (_currentPopups?.Count > 0
                && _currentPopups[0] is LoadingPopupView)
            {
                await _currentPopups[0].CloseAsync();
            }
        }

        public Task ShowPopupAsync(AlertPopupModel alertPopupModel)
        {
            if (_currentPopups.Count > 0)
                throw new InvalidOperationException(AppConstants.PopupExceptionMessage);

            var popup = CreateAlertPopup(alertPopupModel);

            _ = Application.Current?.Windows[0].Page?.ShowPopupAsync(popup);

            return Task.CompletedTask;
        }

        public Task ShowLoadingAsync()
        {
            if (_currentPopups.Count > 0)
                throw new InvalidOperationException(AppConstants.PopupExceptionMessage);

            var popup = CreateLoadingPopup();

            _ = Application.Current?.Windows[0].Page?.ShowPopupAsync(popup);

            return Task.CompletedTask;
        }

        private BasePopup CreateLoadingPopup()
        {
            BasePopup? popup = new LoadingPopupView();

            popup.Opened += Popup_Opened;
            popup.Closed += Popup_Closed;

            return popup;
        }

        private BasePopup CreateAlertPopup(AlertPopupModel alertPopupModel)
        {
            BasePopup? popup = new AlertPopupView(alertPopupModel);

            popup.Opened += Popup_Opened;
            popup.Closed += Popup_Closed;

            return popup;
        }

        private void Popup_Closed(object? sender, CommunityToolkit.Maui.Core.PopupClosedEventArgs e)
        {
            if (sender is BasePopup popup)
            {
                popup.Closed += Popup_Closed;
                _currentPopups.Remove(popup);
            }
        }
        private void Popup_Opened(object? sender, CommunityToolkit.Maui.Core.PopupClosedEventArgs e)
        {
            if (sender is BasePopup popup)
            {
                popup.Opened += Popup_Opened;
                _currentPopups.Add(popup);
            }
        }
    }
}
