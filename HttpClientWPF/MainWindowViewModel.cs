using HttpClientService;
using LoggerService;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;

namespace HttpClientWPF
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        public enum AuthenticationMethodType
        {
            Basic,
            Digest,
            Anonymous,
        }

        public ReactiveProperty<string> HostName { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<int> PortNo { get; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<string> Path { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<int> TimeoutSeconds { get; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<string> LogText { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<string> StatusMessage { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<AuthenticationMethodType> AuthenticationMethod { get; } = new(AuthenticationMethodType.Basic);
        public ReactiveProperty<string> User { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<string> Password { get; } = new ReactiveProperty<string>(string.Empty);

        public ReactiveCommand LoadedCommand { get; } = new();
        public ReactiveCommand SaveCommand { get; } = new ReactiveCommand();
        public ReactiveCommand SendCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ClearMessageCommand { get; } = new ReactiveCommand();


        private static class CommunicationLog
        {
            public const string Directory = @"logs";
            public const string FilePath = @"Communication.log";
        }

        private readonly CompositeDisposable _disposables = new();

        private readonly IClient _client;

        private readonly ILog4netAdapter _logger =
            Log4netAdapterFactory.Create(logDirectoryName: CommunicationLog.Directory, logFileName: CommunicationLog.FilePath);

        private readonly ILogFileWatcher _logFileWatcher =
            LogFileWatcherFactory.Create(logDirectoryName: CommunicationLog.Directory, logFileName: CommunicationLog.FilePath);

        public MainWindowViewModel(IClient client)
        {
            SaveCommand.Subscribe(this.OnSaveButtonClicked).AddTo(_disposables);
            SendCommand.Subscribe(this.OnSendButtonClicked).AddTo(_disposables);
            LoadedCommand.Subscribe(this.OnLoaded).AddTo(_disposables);
            ClearMessageCommand.Subscribe(this.ClearMessage).AddTo(_disposables);
            this._client = client;

            // 通信履歴ファイルの監視を開始
            _logFileWatcher.FileChanged += OnLogFileChanged;
        }

        // todo: 画面に入力されている設定と保存済の設定に差分がある場合は、送信ボタンを無効化するようにする

        private async void OnLoaded()
        {
            try
            {
                ConfigData configData = ConfigManager.GetConfigData();
                this.HostName.Value = configData.Host;
                this.PortNo.Value = int.Parse(configData.Port);
                this.Path.Value = configData.Path;
                this.TimeoutSeconds.Value = configData.TimeoutSeconds;
                // 未設定や不正値は Basic を設定する
                if (Enum.TryParse<AuthenticationMethodType>(configData.AuthenticationMethod, ignoreCase: true, out var method))
                {
                    AuthenticationMethod.Value = method;
                }
                else
                {
                    AuthenticationMethod.Value = AuthenticationMethodType.Basic;
                }
                this.User.Value = configData.User;
                this.Password.Value = configData.Password;
                this.LogText.Value = await _logFileWatcher.ReadLogFileContentAsync();
            }
            catch (Exception e)
            {
                _logger.Error("Loadに失敗しました。", e);
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
                    TimeoutSeconds = this.TimeoutSeconds.Value,
                    AuthenticationMethod = this.AuthenticationMethod.Value.ToString(),
                    User = this.User.Value,
                    Password = this.Password.Value
                };
                ConfigManager.SaveConfigData(configData);

                StatusMessage.Value = "設定を保存しました。";
            }
            catch (Exception e)
            {
                _logger.Error("設定の保存に失敗しました。", e);
                StatusMessage.Value = "設定の保存に失敗しました。";
            }
        }

        private void OnSendButtonClicked()
        {
            try
            {
                var message = _client.GetMessage(string.Empty);
                _logger.Info($"受信メッセージ: {message}");
            }
            catch (Exception e)
            {
                _logger.Error("送信に失敗しました。", e);
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
