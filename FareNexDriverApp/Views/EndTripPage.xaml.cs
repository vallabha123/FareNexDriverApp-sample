using FareNexDriverApp.Core.ViewModels;
using FareNexDriverApp.Views.BasePages;

namespace FareNexDriverApp.Views;

public partial class EndTripPage : BasePage
{
    public EndTripPage(EndTripPageViewModel endTripPageViewModel) : base(endTripPageViewModel)
    {
        InitializeComponent();
        BindingContext = endTripPageViewModel;
    }
}