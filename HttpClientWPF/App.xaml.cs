using System.Windows;
using HttpClientService;
using LoggerService;

namespace HttpClientWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private static class CommunicationLog
        {
            public const string Directory = @"logs";
            public const string FilePath = @"Communication.log";
        }

        protected override Window CreateShell() => Container.Resolve<MainWindow>();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(
                new LoggerOptions { LogDirectoryName = CommunicationLog.Directory, LogFileName = CommunicationLog.FilePath });

            containerRegistry.RegisterSingleton<IClient, Client>();
            containerRegistry.RegisterSingleton<ILogFileWatcher, LogFileWatcher>();
            containerRegistry.RegisterSingleton<ILog4netAdapter, Log4netAdapter>();
        }
    }
}
