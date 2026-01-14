using FareNexDriverApp;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Utilities;
using FareNexDriverApp.Core.ViewModels;
using System.Diagnostics;

namespace FareNextDriverApp
{
    public partial class App : Application
    {
        readonly AppShellViewModel _appShellViewModel;

        public App(
            AppShellViewModel appShellViewModel,
            ILogService<App> logService)
        {
            logService.LogMethodEntry();

            InitializeComponent();

            _appShellViewModel = appShellViewModel;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            logService.LogInformation(AppConstants.ApplicationNewLine);
            logService.LogInformation(
                "Application Version - " + AppInfo.Current.VersionString);
            logService.LogInformation(AppConstants.ApplicationNewLine);

            logService.LogMethodExit();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell(_appShellViewModel));
        }

        /// <summary>
        /// Handle uncaught exceptions raised inside application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomain_UnhandledException(
            object sender,
            UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
#if DEBUG
                Debug.WriteLine(e.ExceptionObject.ToString());
#endif
                ILogService<App>? logService =
                    Current?.Handler?.GetService<ILogService<App>>();

                logService?.LogException(ex);
            }

            Environment.Exit(1);
        }

        /// <summary>
        /// Handle uncaught exceptions raised inside application tasks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TaskScheduler_UnobservedTaskException(
            object? sender,
            UnobservedTaskExceptionEventArgs e)
        {
#if DEBUG
            Debug.WriteLine(e.Exception.ToString());
#endif
            ILogService<App>? logService =
                Current?.Handler?.GetService<ILogService<App>>();

            logService?.LogException(e.Exception);
            e.SetObserved();
        }
    }
}
