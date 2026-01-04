using System.Windows;
using ConfigService;
using HttpClientService;
using LoggerService;

namespace HttpClientWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell() => Container.Resolve<MainWindow>();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(
                new LoggerOptions { LogDirectoryName = @"logs", LogFileName = @"Communication.log" });

            containerRegistry.RegisterSingleton<IClient, Client>();
            containerRegistry.RegisterSingleton<ILogFileWatcher, LogFileWatcher>();
            containerRegistry.RegisterSingleton<ILog4netAdapter, Log4netAdapter>();
            containerRegistry.RegisterSingleton<IConfigService, ConfigManager>();
            containerRegistry.RegisterSingleton<IConfigService>(() => new ConfigManager(filePath: @"external_setting_file.json"));
        }
    }
}
