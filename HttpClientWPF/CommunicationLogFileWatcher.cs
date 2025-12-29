using System.IO;
using System.Text;

namespace HttpClientWPF
{
    public sealed class CommunicationLogFileWatcher : IDisposable
    {
        private readonly FileSystemWatcher _fileWatcher;
        private const string LogDirectory = @"logs";
        private const string LogFilePath = @"Communication.log";
        private const int ReadFileRetryCount = 5;

        public event EventHandler<string>? FileChanged;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CommunicationLogFileWatcher()
        {
            _fileWatcher = new FileSystemWatcher
            {
                Path = Path.Combine(AppContext.BaseDirectory, LogDirectory),    // 監視対象のディレクトリ
                Filter = LogFilePath,   // 監視対象のファイル名
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size, // 監視対象の変更内容
                IncludeSubdirectories = false,      // サブディレクトリは監視しない
                EnableRaisingEvents = true          // 監視を開始
            };


            // イベントハンドラの登録
            _fileWatcher.Created += OnFileChanged;
            _fileWatcher.Changed += OnFileChanged;
        }

        /// <summary>ファイル変更時処理</summary>
        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            /* 【連続発火・ロック対策】
             * 一回のファイル保存で複数回呼ばれることがあるため、少し待つ。
             * あと、少し待ってから書き込みが終わってからファイルを読み込むようにする。
             */
            await Task.Delay(100);

            try
            {
                string content = await ReadFileWithRetryAsync(Path.Combine(AppContext.BaseDirectory, LogDirectory, LogFilePath));
                FileChanged?.Invoke(this, content);
            }
            catch
            {
            }
        }


        /// <summary>ファイル読込み時処理</summary>
        /// <remarks>一回でファイルを読み込めない場合があるので複数回リトライする</remarks>
        private static async Task<string> ReadFileWithRetryAsync(string path)
        {
            for (int i = 0; i < ReadFileRetryCount; i++)
            {
                try
                {
                    using var stream = new FileStream(
                        path,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite);

                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    return await reader.ReadToEndAsync();
                }
                catch (IOException)
                {
                    await Task.Delay(100);
                }
            }

            throw new IOException("ファイルを読み取れませんでした。");
        }

        public void Dispose()
        {
            _fileWatcher?.Dispose();
        }
    }
}
