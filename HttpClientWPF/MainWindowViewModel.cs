using HttpClientLibraty;
using log4net;
using Reactive.Bindings;

namespace HttpClientWPF
{
    public class MainWindowViewModel : BindableBase
    {
        public ReactiveProperty<string> HostName { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<int> PortNo { get; } = new ReactiveProperty<int>();
        public ReactiveProperty<string> Path { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<int> TimeoutSeconds { get; } = new ReactiveProperty<int>();
        public ReactiveProperty<string> LogText { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> StatusMessage { get; } = new ReactiveProperty<string>(string.Empty);

        public ReactiveCommand LoadedCommand { get; } = new();
        public ReactiveCommand SaveCommand { get; } = new ReactiveCommand();
        public ReactiveCommand SendCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ClearMessageCommand { get; } = new ReactiveCommand();


        private ILog Logger { get; } = LogManager.GetLogger(typeof(MainWindowViewModel));


        private CommunicationLogFileWatcher _logFileWatcher;

        public MainWindowViewModel()
        {
            SaveCommand.Subscribe(this.OnSaveButtonClicked);
            SendCommand.Subscribe(this.OnSendButtonClicked);
            LoadedCommand.Subscribe(this.OnLoaded);
            ClearMessageCommand.Subscribe(this.ClearMessage);


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
            catch (Exception)
            {
                throw;
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
            catch (Exception)
            {
                StatusMessage.Value = "設定の保存に失敗しました。";
                throw;
            }
        }

        private void OnSendButtonClicked()
        {
            try
            {
                var message = Client.Instance.GetMessage(string.Empty);
                Logger.Info($"受信メッセージ: {message}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OnLogFileChanged(object? sender, string content) => LogText.Value = content;


        private void ClearMessage()
        {
            StatusMessage.Value = string.Empty;
        }
    }
}
