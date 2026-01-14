using FareNexDriverApp.Core.ViewModels;

namespace FareNexDriverApp
{
    public partial class AppShell : Shell
    {
        public AppShell(AppShellViewModel appShellViewModel)
        {
            InitializeComponent();
            BindingContext = appShellViewModel;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}

