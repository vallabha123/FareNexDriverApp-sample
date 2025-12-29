using Android.Content.Res;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CommunityToolkit.Maui;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Localizations;
using FareNexDriverApp.Core.Models.Configurations;
using FareNexDriverApp.Core.Services;
using FareNexDriverApp.Core.ViewModels;
using FareNexDriverApp.Extensions;
using FareNexDriverApp.IOC;
using FareNexDriverApp.Services;
using FareNexDriverApp.Views;
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
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                    fonts.AddFont("Montserrat-Bold.ttf", "MontserratBold");
                    fonts.AddFont("Montserrat-Medium.ttf", "MontserratMedium");
                    fonts.AddFont("Montserrat-Regular.ttf", "MontserratRegular");
                    fonts.AddFont("Montserrat-Semibold.ttf", "MontserratSemibold");

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
#if DEBUG
            using Stream? stream = FileSystem.OpenAppPackageFileAsync("AppSettings.Development.json").Result;
#elif QA
            using Stream? stream = FileSystem.OpenAppPackageFileAsync("AppSettings.UAT.json").Result;
#elif RELEASE
            using Stream? stream = FileSystem.OpenAppPackageFileAsync("AppSettings.Production.json").Result;
#endif
            if (stream is not null)
            {
                return new ConfigurationBuilder().AddJsonStream(stream).Build();
            }
            else
            {
                return new ConfigurationBuilder().Build();
            }
        }

        private static MauiAppBuilder ConfigureEnvironment(this MauiAppBuilder builder, IConfigurationRoot configurationRoot)
        {
            builder.Configuration.AddConfiguration(configurationRoot);
            return builder;
        }

        private static ILoggingBuilder ConfigureLoggingProvider(this ILoggingBuilder loggingBuilder, IConfigurationRoot configurationRoot)
        {
            var loggerConfig = new LoggerConfiguration()
                                        .MinimumLevel.Debug()
                                        .WriteTo.Conditional(condition: logEvent => logEvent.Level >= LogEventLevel.Information && logEvent.Level != LogEventLevel.Warning,
                                                configureSink: x => x.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                                                                                , configurationRoot[AppConfiguration.ConfigurationValues[Core.Utilities.ConfigurationKeys.SerilogLogFileName]] ?? string.Empty),
                                                                                 rollingInterval: RollingInterval.Day
                                                                                 , restrictedToMinimumLevel: LogEventLevel.Information))
                                         .CreateLogger();

            loggingBuilder.AddSerilog(loggerConfig);
            return loggingBuilder;
        }

        #endregion

        #region Register Views and Services

        private static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
        {
            builder.Services.AddTransientPageWithShellRoute<AppShell, AppShellViewModel>();
            builder.Services.AddTransientPageWithShellRoute<HomePage, HomePageViewModel>();
            builder.Services.AddTransientPageWithShellRoute<TripSelectionPage, TripSelectionPageViewModel>();
            builder.Services.AddTransientPageWithShellRoute<IdleScreenPage, IdleScreenPageViewModel>();
            builder.Services.AddTransientPageWithShellRoute<LogOutPage, LogOutPageViewModel>();
            builder.Services.AddTransientPageWithShellRoute<LogInPage, LogInPageViewModel>();





            return builder;
        }

        private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
            builder.Services.AddSingleton(typeof(ILogService<>), typeof(LogService<>));
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            return builder;
        }
        #endregion

        #region Configure Handlers 

        /// <summary>
        /// Add configuration root to configuration
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        private static MauiAppBuilder ConfigureHandlers(this MauiAppBuilder builder)
        {
            builder.ConfigureMauiHandlers((handlers) =>
            {
                Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                {
#if ANDROID
                    handler.PlatformView.Background = null;
#endif
                });


                Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("EntryNoUnderline", (handler, view) =>
                {
#if ANDROID
                    handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
                });
            });
            return builder;
        }

        #endregion
    }
}