using CommunityToolkit.Maui.Core.Handlers;
using CommunityToolkit.Maui.Views;

namespace FareNexDriverApp.Views
{
    public class BasePopup : Popup
    {
        public BasePopup()
        {
            // Background color with opacity
            this.Color = Colors.White.WithAlpha(0.9f);

            // Popup size
            this.Size = new Size(1280, 800);

            // Android window settings
            PopupHandler.PopUpMapper.AppendToMapping("test", (handler, view) =>
            {
                if (handler.PlatformView.Window != null)
                {
                    handler.PlatformView.Window?
                        .SetFlags(
                            Android.Views.WindowManagerFlags.NotFocusable,
                            Android.Views.WindowManagerFlags.NotFocusable);

                    handler.PlatformView.Window?.SetDimAmount(0f);
                    handler.PlatformView.Window?.SetElevation(0f);
                }
            });
        }
    }
}
