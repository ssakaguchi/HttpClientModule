using System.IO;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace HttpClientWPF
{
    public sealed class Log4netAdapter: ILog4netAdapter
    {
        private ILog Logger { get; } = LogManager.GetLogger(typeof(Log4netAdapter));

        public Log4netAdapter()
        {
            // ログディレクトリ作成
            var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);

            // レイアウト
            var layout = new PatternLayout
            {
                ConversionPattern = "%date - %message%newline"
            };
            layout.ActivateOptions();

            // RollingFileAppender
            var appender = new RollingFileAppender
            {
                Name = "ConsoleAppender",
                File = Path.Combine(logDir, "Communication.log"),
                AppendToFile = true,
                RollingStyle = RollingFileAppender.RollingMode.Size,
                MaximumFileSize = "10MB",
                MaxSizeRollBackups = 3,
                StaticLogFileName = true,
                LockingModel = new FileAppender.MinimalLock(),
                Layout = layout
            };
            appender.ActivateOptions();

            // Root logger 設定
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.Level = Level.All;
            hierarchy.Root.AddAppender(appender);
            hierarchy.Configured = true;

        }

        public void Info(string message) => Logger.Info(message);

        public void Error(string message) => Logger.Error(message);

        public void Error(string message, Exception ex) => Logger.Error(message, ex);

        public static ILog4netAdapter Create() => new Log4netAdapter();
    }


    public interface ILog4netAdapter
    {
        public void Info(string message);

        public void Error(string message);

        public void Error(string message, Exception ex);
    }
}
