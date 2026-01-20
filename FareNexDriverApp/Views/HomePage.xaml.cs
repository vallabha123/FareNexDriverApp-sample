using CommunityToolkit.Mvvm.Messaging;
using FareNexDriverApp.Core.EventArgs;
using FareNexDriverApp.Core.ViewModels;
using FareNexDriverApp.Views.BasePages;

namespace FareNexDriverApp.Views
{
    public partial class HomePage : BasePage
    {
        public HomePage(HomePageViewModel homePageViewModel)
            : base(homePageViewModel)
        {
            InitializeComponent();
            BindingContext = homePageViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            StrongReferenceMessenger.Default.Unregister<ItemUpdateScrollMessage>(this);

            StrongReferenceMessenger.Default.Register<ItemUpdateScrollMessage>(
                this,
                (sender, args) =>
                {
                    _ = ScrollToFirstItem();
                });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StrongReferenceMessenger.Default.Unregister<ItemUpdateScrollMessage>(this);
        }

        public async Task ScrollToFirstItem()
        {
            await Dispatcher.DispatchAsync(() =>
            {
                paymentsCollectionView.ScrollTo(
                    0,
                    position: ScrollToPosition.Start
                );
            });
        }
    }
}
