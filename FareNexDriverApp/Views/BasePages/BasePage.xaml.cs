
namespace FareNexDriverApp.Views.BasePages;
public partial class BasePage : ContentPage
{
    public BasePage(object vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
