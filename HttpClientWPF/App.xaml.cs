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
            containerRegistry.RegisterSingleton<IClient, Client>();
            containerRegistry.RegisterSingleton<ILogFileWatcher>
                (() => new LogFileWatcher(logDirectoryName: @"logs",logFileName: @"Communication.log"));
            containerRegistry.RegisterSingleton<ILog4netAdapter>
                (() => new Log4netAdapter(logDirectoryName: @"logs", logFileName: @"Communication.log"));
            containerRegistry.RegisterSingleton<IConfigService>(() => new ConfigManager(filePath: @"external_setting_file.json"));
        }
    }
}
