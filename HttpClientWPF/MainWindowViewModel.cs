using HttpClientLibraty;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;

namespace HttpClientWPF
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        public ReactiveProperty<string> HostName { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<int> PortNo { get; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<string> Path { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<int> TimeoutSeconds { get; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<string> LogText { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<string> StatusMessage { get; } = new ReactiveProperty<string>(string.Empty);

        public ReactiveCommand LoadedCommand { get; } = new();
        public ReactiveCommand SaveCommand { get; } = new ReactiveCommand();
        public ReactiveCommand SendCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ClearMessageCommand { get; } = new ReactiveCommand();


        private readonly CompositeDisposable _disposables = new();

        //private readonly ILog4netAdapter Logger = Log4netAdapter.Create();
        private readonly LoggerService.ILog4netAdapter Logger = LoggerService.Log4netAdapter.Create();


        private CommunicationLogFileWatcher _logFileWatcher;

        public MainWindowViewModel()
        {
            SaveCommand.Subscribe(this.OnSaveButtonClicked).AddTo(_disposables);
            SendCommand.Subscribe(this.OnSendButtonClicked).AddTo(_disposables);
            LoadedCommand.Subscribe(this.OnLoaded).AddTo(_disposables);
            ClearMessageCommand.Subscribe(this.ClearMessage).AddTo(_disposables);


            // 通信履歴ファイルの監視を開始
            _logFileWatcher = new CommunicationLogFileWatcher();
            _logFileWatcher.FileChanged += OnLogFileChanged;
        }

        // todo: 画面に入力されている設定と保存済の設定に差分がある場合は、送信ボタンを無効化するようにする

        private void OnLoaded()
        {
            try
            {
                ConfigData configData = ConfigManager.GetConfigData();
                this.HostName.Value = configData.Host;
                this.PortNo.Value = int.Parse(configData.Port);
                this.Path.Value = configData.Path;
                this.TimeoutSeconds.Value = configData.TimeoutSeconds;
            }
            catch (Exception e)
            {
                Logger.Error("Loadに失敗しました。", e);
                StatusMessage.Value = "Loadに失敗しました。";
            }
        }

        private void OnSaveButtonClicked()
        {
            try
            {
                var configData = new ConfigData
                {
                    Host = this.HostName.Value,
                    Port = this.PortNo.Value.ToString(),
                    Path = this.Path.Value,
                    TimeoutSeconds = this.TimeoutSeconds.Value
                };
                ConfigManager.SaveConfigData(configData);

                StatusMessage.Value = "設定を保存しました。";
            }
            catch (Exception e)
            {
                Logger.Error("設定の保存に失敗しました。", e);
                StatusMessage.Value = "設定の保存に失敗しました。";
            }
        }

        private void OnSendButtonClicked()
        {
            try
            {
                var message = Client.Instance.GetMessage(string.Empty);
                Logger.Info($"受信メッセージ: {message}");
            }
            catch (Exception e)
            {
                Logger.Error("送信に失敗しました。", e);
                StatusMessage.Value = "送信に失敗しました。";
            }
        }

        private void OnLogFileChanged(object? sender, string content) => LogText.Value = content;

        private void ClearMessage() => StatusMessage.Value = string.Empty;

        public void Dispose()
        {
            _logFileWatcher.FileChanged -= OnLogFileChanged;
            _logFileWatcher.Dispose();
            _disposables.Dispose();
        }
    }
}
