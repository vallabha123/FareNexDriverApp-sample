
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FareNexDriverApp.Core.ViewModels.BaseViewModels
{
    public abstract partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _title;

        protected internal bool _isLoading;

        [RelayCommand]
        public virtual Task OnAppearing()
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        public virtual Task OnDisappearing()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// called to prepare the viewmodel with navigation data
        /// </summary>
        /// <param name="query"></param>
        public virtual void Prepare(IDictionary<string, object> query)
        {

        }

        /// <summary>
        /// Called when a page is unloaded from application
        /// </summary>
        [RelayCommand]
        public virtual Task OnPageUnloaded()
        {
            return Task.CompletedTask;
        }
    }
}
