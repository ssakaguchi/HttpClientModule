using System.Reactive.Linq;
using ConfigService;
using HttpClientService;
using HttpClientWPF.ConfigMapper;
using HttpClientWPF.FileDialogService;
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
        private readonly ILoggerService _logger;
        private readonly ILogFileWatcher _logFileWatcher;
        private readonly IConfigService _configService;
        private readonly IConfigMapper _configMapper;
        private readonly IOpenFileDialogService _openFileDialogService;

        public MainWindowViewModel(IClient client,
                                   ILoggerService log4NetAdapter,
                                   ILogFileWatcher logFileWatcher,
                                   IConfigService configService,
                                   IConfigMapper configMapper,
                                   IOpenFileDialogService fileDialogService)
        {
            UploadFileSelectCommand.Subscribe(this.OnUploadFileSelectButtonClicked).AddTo(_disposables);
            SaveCommand.Subscribe(this.OnSaveButtonClicked).AddTo(_disposables);
            SendCommand.Subscribe(_ => this.OnSendButtonClicked().ConfigureAwait(false)).AddTo(_disposables);
            PostCommand.Subscribe(_ => this.OnPostButtonClicked().ConfigureAwait(false)).AddTo(_disposables);
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
            _configMapper = configMapper;
            _openFileDialogService = fileDialogService;

            // 通信履歴ファイルの監視を開始
            _logFileWatcher.FileChanged += OnLogFileChanged;
        }

        private async void OnLoaded()
        {
            try
            {
                var config = _configService.Load();
                _configMapper.ApplyTo(this, config);

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
                _openFileDialogService.Title = "アップロードファイルの選択";
                _openFileDialogService.Filter = "すべてのファイル (*.*)|*.*";

                bool? result = _openFileDialogService.OpenFileDialog();
                if (result == true)
                {
                    this.UploadFilePath.Value = _openFileDialogService.FilePath;
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

                var config = _configMapper.CreateFrom(this);
                _configService.Save(config);

                this.UpdateEnabled();

                StatusMessage.Value = "設定を保存しました。";
            }
            catch (Exception e)
            {
                _logger.Error("設定の保存に失敗しました。", e);
                StatusMessage.Value = "設定の保存に失敗しました。";
            }
        }

        private async Task OnSendButtonClicked()
        {
            try
            {
                ClearStatusMessage();
                
                string message =　await _client.GetAsync(string.Empty);
                _logger.Info($"受信データ:\r\n{message}");
            }
            catch (Exception e)
            {
                _logger.Error("GET送信に失敗しました。", e);
                StatusMessage.Value = "送信に失敗しました。";
            }
        }


        private async Task OnPostButtonClicked()
        {
            try
            {
                ClearStatusMessage();

                string command = "UploadFile";
                var message = await _client.PostAsync(command);
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
            ConfigData config = _configMapper.CreateFrom(this);
            bool existsDifference = _configService.ExistsConfigDifference(config);
            SaveCommandEnabled.Value = existsDifference;
            SendCommandEnabled.Value = !existsDifference;
            PostCommandEnabled.Value = !existsDifference;

            IsUserEnabled.Value = AuthenticationMethod.Value == AuthenticationMethodType.Basic;
            IsPasswordEnabled.Value = AuthenticationMethod.Value == AuthenticationMethodType.Basic;
        }

        private void ClearStatusMessage() => StatusMessage.Value = string.Empty;

        public void Dispose()
        {
            _logFileWatcher.FileChanged -= OnLogFileChanged;
            _logFileWatcher.Dispose();
            _disposables.Dispose();
        }
    }
}
