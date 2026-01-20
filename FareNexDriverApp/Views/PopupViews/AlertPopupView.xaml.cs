using FareNexDriverApp.Core.Models.CustomModels;

namespace FareNexDriverApp.Views.PopupViews;

public partial class AlertPopupView : BasePopup
{
    public AlertPopupView(AlertPopupModel alertPopupModel)
    {
        InitializeComponent();
        BindingContext = alertPopupModel;
    }

    private void DismissButton_Clicked(object sender, EventArgs e)
    {
        this.CloseAsync();
    }
}
