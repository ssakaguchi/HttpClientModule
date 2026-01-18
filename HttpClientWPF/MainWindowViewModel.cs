using System.Reactive.Linq;
using ConfigService;
using HttpClientService;
using LoggerService;
using Microsoft.Win32;
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
            Anonymous,
        }

        public ReactiveProperty<bool> UseHttps { get; } = new ReactiveProperty<bool>(false);
        public ReactiveProperty<string> HostName { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<int> PortNo { get; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<string> Path { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<string> Query { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<int> TimeoutSeconds { get; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<string> LogText { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<string> StatusMessage { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<AuthenticationMethodType> AuthenticationMethod { get; } = new(AuthenticationMethodType.Basic);
        public ReactiveProperty<string> User { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<string> Password { get; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<string> UploadFilePath { get; } = new ReactiveProperty<string>(string.Empty);

        public ReactiveCommand LoadedCommand { get; } = new();
        public ReactiveCommand SaveCommand { get; } = new ReactiveCommand();
        public ReactiveCommand SendCommand { get; } = new ReactiveCommand();
        public ReactiveCommand PostCommand { get; } = new ReactiveCommand();
        public ReactiveCommand UploadFileSelectCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ClearMessageCommand { get; } = new ReactiveCommand();

        public ReactiveProperty<bool> IsUserEnabled { get; } = new ReactiveProperty<bool>(true);
        public ReactiveProperty<bool> IsPasswordEnabled { get; } = new ReactiveProperty<bool>(true);

        
        public ReactiveProperty<bool> SaveCommandEnabled { get; } = new ReactiveProperty<bool>(true);
        public ReactiveProperty<bool> SendCommandEnabled { get; } = new ReactiveProperty<bool>(true);
        public ReactiveProperty<bool> PostCommandEnabled { get; } = new ReactiveProperty<bool>(true);

        private readonly CompositeDisposable _disposables = new();
        private readonly IClient _client;
        private readonly ILog4netAdapter _logger;
        private readonly ILogFileWatcher _logFileWatcher;
        private readonly IConfigService _configService;

        public MainWindowViewModel(IClient client, ILog4netAdapter log4NetAdapter, ILogFileWatcher logFileWatcher, IConfigService configService)
        {
            UploadFileSelectCommand.Subscribe(this.OnUploadFileSelectButtonClicked).AddTo(_disposables);
            SaveCommand.Subscribe(this.OnSaveButtonClicked).AddTo(_disposables);
            SendCommand.Subscribe(this.OnSendButtonClicked).AddTo(_disposables);
            PostCommand.Subscribe(this.OnPostButtonClicked).AddTo(_disposables);
            LoadedCommand.Subscribe(this.OnLoaded).AddTo(_disposables);
            ClearMessageCommand.Subscribe(this.ClearStatusMessage).AddTo(_disposables);

            UseHttps.Skip(1).Subscribe(x => { this.UpdateEnabled(); }).AddTo(_disposables);
            HostName.Skip(1).Subscribe(x => { this.UpdateEnabled(); }).AddTo(_disposables);
            PortNo.Skip(1).Subscribe(x => { this.UpdateEnabled(); }).AddTo(_disposables);
            Path.Skip(1).Subscribe(x => { this.UpdateEnabled(); }).AddTo(_disposables);
            Query.Skip(1).Subscribe(x => { this.UpdateEnabled(); }).AddTo(_disposables);
            TimeoutSeconds.Skip(1).Subscribe(x => { this.UpdateEnabled(); }).AddTo(_disposables);
            AuthenticationMethod.Skip(1).Subscribe(x => { this.UpdateEnabled(); }).AddTo(_disposables);
            User.Skip(1).Subscribe(x => { this.UpdateEnabled(); }).AddTo(_disposables);
            Password.Skip(1).Subscribe(x => { this.UpdateEnabled(); }).AddTo(_disposables);
            UploadFilePath.Skip(1).Subscribe(x => { this.UpdateEnabled(); }).AddTo(_disposables);

            _client = client;
            _logger = log4NetAdapter;
            _logFileWatcher = logFileWatcher;
            _configService = configService;

            // 通信履歴ファイルの監視を開始
            _logFileWatcher.FileChanged += OnLogFileChanged;
        }

        private async void OnLoaded()
        {
            try
            {
                var configData = _configService.Load();
                this.UseHttps.Value = configData.Scheme == "https" ? true : false;
                this.HostName.Value = configData.Host;
                this.PortNo.Value = int.Parse(configData.Port);
                this.Path.Value = configData.Path;
                this.Query.Value = configData.Query;
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
                this.UploadFilePath.Value = configData.UploadFilePath;

                this.LogText.Value = await _logFileWatcher.ReadLogFileContentAsync();

                this.UpdateEnabled();
            }
            catch (Exception e)
            {
                _logger.Error("Loadに失敗しました。", e);
                StatusMessage.Value = "Loadに失敗しました。";
            }
        }

        private void OnUploadFileSelectButtonClicked()
        {
            try
            {
                OpenFileDialog openFileDialog = new()
                {
                    Title = "アップロードファイルの選択",
                    Filter = "すべてのファイル (*.*)|*.*",
                };

                bool? result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    this.UploadFilePath.Value = openFileDialog.FileName;
                }
            }
            catch (Exception e)
            {
                _logger.Error("アップロードファイルの選択に失敗しました。", e);
                StatusMessage.Value = "アップロードファイルの選択に失敗しました。";
            }
        }

        private void OnSaveButtonClicked()
        {
            try
            {
                ClearStatusMessage();

                var configData = this.CreateInputConfigData();
                _configService.Save(configData);

                this.UpdateEnabled();

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
                ClearStatusMessage();
                
                string message = _client.GetMessage(string.Empty);
                _logger.Info($"受信データ:\r\n{message}");
            }
            catch (Exception e)
            {
                _logger.Error("GET送信に失敗しました。", e);
                StatusMessage.Value = "送信に失敗しました。";
            }
        }


        private void OnPostButtonClicked()
        {
            try
            {
                ClearStatusMessage();

                string command = "UploadFile";
                var message = _client.Post(command);
                _logger.Info($"受信データ:\r\n{message}");
            }
            catch (Exception e)
            {
                _logger.Error("POST送信に失敗しました。", e);
                StatusMessage.Value = "送信に失敗しました。";
            }
        }

        private void OnLogFileChanged(object? sender, string content) => LogText.Value = content;

        private void UpdateEnabled()
        {
            ConfigData configData = this.CreateInputConfigData();
            bool existsDifference = _configService.ExistsConfigDifference(configData);
            SaveCommandEnabled.Value = existsDifference;
            SendCommandEnabled.Value = !existsDifference;
            PostCommandEnabled.Value = !existsDifference;

            IsUserEnabled.Value = AuthenticationMethod.Value == AuthenticationMethodType.Basic;
            IsPasswordEnabled.Value = AuthenticationMethod.Value == AuthenticationMethodType.Basic;
        }

        private void ClearStatusMessage() => StatusMessage.Value = string.Empty;

        private ConfigData CreateInputConfigData()
        {
            return new ConfigData
            {
                Scheme = this.UseHttps.Value ? "https" : "http",
                Host = this.HostName.Value,
                Port = this.PortNo.Value.ToString(),
                Path = this.Path.Value,
                Query = this.Query.Value,
                TimeoutSeconds = this.TimeoutSeconds.Value,
                AuthenticationMethod = this.AuthenticationMethod.Value.ToString(),
                User = this.User.Value,
                Password = this.Password.Value,
                UploadFilePath = this.UploadFilePath.Value,
            };
        }

        public void Dispose()
        {
            _logFileWatcher.FileChanged -= OnLogFileChanged;
            _logFileWatcher.Dispose();
            _disposables.Dispose();
        }
    }
}
