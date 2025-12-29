
using FareNexDriverApp.Core.ViewModels;
using FareNexDriverApp.Views.BasePages;

namespace FareNexDriverApp.Views;
public partial class HomePage : BasePage
{
    public HomePage(HomePageViewModel vm) : base(vm) { InitializeComponent(); }
}
