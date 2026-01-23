using Android.Content.Res;
using Android.Graphics.Drawables;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CommunityToolkit.Maui;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Localizations;
using FareNexDriverApp.Core.Models.Configurations;
using FareNexDriverApp.Core.Services;
using FareNexDriverApp.Core.Utilities;
using FareNexDriverApp.Core.ViewModels;
using FareNexDriverApp.Extensions;
using FareNexDriverApp.IOC;
using FareNexDriverApp.Platforms.Android.PlatformServices;
using FareNexDriverApp.Services;
using FareNexDriverApp.Views;
using FareNextDriverApp.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace FareNexDriverApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            var appConfig = GetConfigurationFile();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureEssentials()
                .ConfigureEnvironment(appConfig)
                .RegisterServices()
                .RegisterViews()
                .ConfigureHandlers()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Montserrat-Regular.ttf", "MontserratRegular");
                    fonts.AddFont("Montserrat-Bold.ttf", "MontserratBold");
                    fonts.AddFont("Montserrat-Medium.ttf", "MontserratMedium");
                    fonts.AddFont("Montserrat-SemiBold.ttf", "MontserratSemiBold");
                })
                .ConfigureContainer(new AutofacServiceProviderFactory(), autofacBuilder =>
                {
                    autofacBuilder.RegisterModule(new RegisterModule());
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Logging.ConfigureLoggingProvider(appConfig);

            return builder.Build();
        }

        #region InitializeConfigs

        private static IConfigurationRoot GetConfigurationFile()
        {
#if DEBUG || QA
            using Stream? stream =
                FileSystem.OpenAppPackageFileAsync("appsettings.Development.json").Result;
#elif ACC
            using Stream? stream =
                FileSystem.OpenAppPackageFileAsync("appsettings.UAT.json").Result;
#elif RELEASE
            using Stream? stream =
                FileSystem.OpenAppPackageFileAsync("appsettings.Production.json").Result;
#endif
            if (stream is not null)
            {
                return new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();
            }

            return new ConfigurationBuilder().Build();
        }

        private static MauiAppBuilder ConfigureEnvironment(
            this MauiAppBuilder builder,
            IConfigurationRoot configurationRoot)
        {
            builder.Configuration.AddConfiguration(configurationRoot);
            return builder;
        }

        private static ILoggingBuilder ConfigureLoggingProvider(
            this ILoggingBuilder loggingBuilder,
            IConfigurationRoot configurationRoot)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Conditional(
                    logEvent =>
                        logEvent.Level >= LogEventLevel.Information &&
                        logEvent.Level != LogEventLevel.Warning,
                    configureSink => configureSink.File(
                        path: Path.Combine(
                            AppConstants.AppExternalDataFolderPath(),
                            AppConstants.AppLogDataFolderName,
                            configurationRoot[
                                Core.Utilities.ConfigurationKeys.SerilogLogFileName
                            ] ?? string.Empty
                        ),
                        rollingInterval: RollingInterval.Hour,
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        retainedFileCountLimit: 24
                    ))
                .CreateLogger();

            loggingBuilder.AddSerilog(loggerConfig);
            return loggingBuilder;
        }

        #endregion

        #region Register Views and Services

        private static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
        {
            builder.Services.AddSingletonPageWithShellRoute<AppShell, AppShellViewModel>();
            builder.Services.AddTransientPageWithShellRoute<HomePage, HomePageViewModel>();
            builder.Services.AddTransientPageWithShellRoute<TripSelectionPage, TripSelectionPageViewModel>();
            builder.Services.AddTransientPageWithShellRoute<DriverPinPage, DriverPinPageViewModel>();
            builder.Services.AddTransientPageWithShellRoute<EndTripPage, EndTripPageViewModel>();

            return builder;
        }

        private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton(typeof(ILogService<>), typeof(LogService<>));
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
            builder.Services.AddSingleton<IPopupService, Services.PopupService>();
            builder.Services.AddSingleton<ILocationService, LocationService>();

            return builder;
        }

        #endregion

        #region Configure Handlers

        private static MauiAppBuilder ConfigureHandlers(this MauiAppBuilder builder)
        {
            builder.ConfigureMauiHandlers(handlers =>
            {
                Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping(
                    "NoUnderLine",
                    (handler, view) =>
                    {
#if ANDROID
                        handler.PlatformView.Background = null;
#endif
                    });

                Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(
                    "EntryNoUnderline",
                    (handler, view) =>
                    {
#if ANDROID
                        handler.PlatformView.BackgroundTintList =
                            ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
                    });

                Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping(
                  "NoRippleButton",
                  (handler, view) =>
                  {
#if ANDROID
                      if (handler.PlatformView.Background is RippleDrawable rippleDrawable)
                      {
                          rippleDrawable.SetTintList(null);
                      }
#endif
                  });
            });

            return builder;
        }

        #endregion
       

    }
}
