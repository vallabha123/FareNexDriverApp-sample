namespace FareNexDriverApp.Views.PopupViews;

public partial class LoadingPopupView : BasePopup
{
    public LoadingPopupView()
    {
        InitializeComponent();

        _ = StartFadingAnimation();

        this.Closed -= LoadingPopupView_Closed;
        this.Closed += LoadingPopupView_Closed;
    }

    private void LoadingPopupView_Closed(
        object? sender,
        CommunityToolkit.Maui.Core.PopupClosedEventArgs e)
    {
        loadingImg.AbortAnimation("fade");
    }

    private async Task StartFadingAnimation()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Animation animation = new(a =>
                loadingImg.Opacity = a,
                1, 0.2,
                easing: Easing.Linear);

            Animation reverseAnimation = new(a =>
                loadingImg.Opacity = a,
                0.2, 1,
                easing: Easing.Linear);

            var parentAnimation = new Animation
            {
                { 0, 0.5, animation },
                { 0.5, 1, reverseAnimation }
            };

            parentAnimation.Commit(
                loadingImg,
                "fade",
                length: 1500,
                repeat: () => true);
        });
    }
}
