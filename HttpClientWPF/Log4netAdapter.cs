using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System.IO;

namespace HttpClientWPF
{
    internal class Log4netAdapter
    {
        public static void Configure()
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
    }
}
