
using FareNexDriverApp.Core.ViewModels;
using FareNexDriverApp.Views.BasePages;

namespace FareNexDriverApp.Views;
public partial class DriverPinPage : BasePage
{
    public DriverPinPage(DriverPinPageViewModel vm) : base(vm) 
    { 
        InitializeComponent(); 
        BindingContext = vm;
    }
    private void PinEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue?.Length > 4)
        {
            _ = pinEntry.HideSoftInputAsync(new());
        }
    }
}
