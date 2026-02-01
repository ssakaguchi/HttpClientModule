using ConfigService;
using HttpClientService;
using HttpClientWPF;
using HttpClientWPF.ConfigMapper;
using HttpClientWPF.FileDialogService;
using LoggerService;
using Moq;


namespace MainWindowViewModelTest
{
    public class MainWindowViewModelTests
    {
        private readonly Mock<IClient> _mockClient;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly Mock<ILogFileWatcher> _mockLogFileWatcher;
        private readonly Mock<IConfigService> _mockConfigService;
        private readonly Mock<IConfigMapper> _mockConfigMapper;
        private readonly Mock<IOpenFileDialogService> _mockOpenFileDialogService;
        private readonly MainWindowViewModel _viewModel;

        public MainWindowViewModelTests()
        {
            _mockClient = new Mock<IClient>();
            _mockLogger = new Mock<ILoggerService>();
            _mockLogFileWatcher = new Mock<ILogFileWatcher>();
            _mockConfigService = new Mock<IConfigService>();
            _mockConfigMapper = new Mock<IConfigMapper>();
            _mockOpenFileDialogService = new Mock<IOpenFileDialogService>();

            _viewModel = new MainWindowViewModel(
                _mockClient.Object,
                _mockLogger.Object,
                _mockLogFileWatcher.Object,
                _mockConfigService.Object,
                _mockConfigMapper.Object,
                _mockOpenFileDialogService.Object
            );
        }


        [Fact]
        public void 画面起動に成功_通信ログのデータを画面上のログエリアに表示する()
        {
            // arrange
            var expectedConfig = new ConfigData();
            var expectedLogContent = "Log content";

            _mockConfigService.Setup(x => x.Load()).Returns(expectedConfig);
            _mockLogFileWatcher.Setup(x => x.ReadLogFileContentAsync()).ReturnsAsync(expectedLogContent);

            // act
            _viewModel.LoadedCommand.Execute();

            _mockConfigService.Verify(x => x.Load(), Times.Once);
            _mockConfigMapper.Verify(x => x.ApplyTo(_viewModel, expectedConfig), Times.Once);

            // assert
            // ログ内容が画面に反映されていることを確認
            Assert.Equal(expectedLogContent, _viewModel.LogText.Value);
        }

        [Fact]
        public void 画面起動時に設定情報の読み込みに失敗した場合_エラーログが出力される_画面に指定のエラーメッセージを表示する()
        {
            // arrange
            _mockConfigService.Setup(x => x.Load()).Throws(new Exception("Load error"));

            // act
            _viewModel.LoadedCommand.Execute();

            // assert
            // エラーログが記録されていることを確認
            _mockLogger.Verify(x => x.Error("Loadに失敗しました。", It.IsAny<Exception>()), Times.Once);

            // 画面にエラーメッセージが表示されていることを確認
            Assert.Equal("Loadに失敗しました。", _viewModel.StatusMessage.Value);
        }

        [Fact]
        public void ファイル選択ダイアログでファイルを選択_アップロードファイルプロパティに選択したファイルのパスがセットされる()
        {
            // arrange
            var expectedFilePath = "C:\\test\\file.txt";
            _mockOpenFileDialogService.Setup(x => x.OpenFileDialog()).Returns(true);
            _mockOpenFileDialogService.Setup(x => x.FilePath).Returns(expectedFilePath);

            // act
            _viewModel.UploadFileSelectCommand.Execute();

            // assert
            // アップロードファイルパスがViewModelに反映されていることを確認
            Assert.Equal(expectedFilePath, _viewModel.UploadFilePath.Value);
        }

        [Fact]
        public void Saveボタン押下時に設定の保存に失敗_画面に指定のエラーメッセージを表示する()
        {
            // arrange
            _mockConfigMapper.Setup(x => x.CreateFrom(_viewModel)).Throws(new Exception("Save error"));

            // act
            _viewModel.SaveCommand.Execute();

            // assert
            // エラーログが記録されていることを確認
            _mockLogger.Verify(x => x.Error("設定の保存に失敗しました。", It.IsAny<Exception>()), Times.Once);

            // 画面にエラーメッセージが表示されていることを確認
            Assert.Equal("設定の保存に失敗しました。", _viewModel.StatusMessage.Value);
        }

        [Fact]
        public void 疎通確認ボタン押下時_GET送信を実行_受信データを通信ログに出力する()
        {
            // arrange
            var expectedMessage = "Response data";
            _mockClient.Setup(x => x.GetAsync(string.Empty, new CancellationToken())).ReturnsAsync(expectedMessage);

            // act
            _viewModel.SendCommand.Execute();

            // assert
            // GET送信が実行されていることを確認
            _mockClient.Verify(x => x.GetAsync(string.Empty, new CancellationToken()), Times.Once);

            // 受信データがログに出力されていることを確認
            _mockLogger.Verify(x => x.Info($"受信データ:\r\n{expectedMessage}"), Times.Once);
        }

        [Fact]
        public void 疎通確認ボタン押下時_GET送信を実行_GET送信に失敗した旨を通信ログに出力する()
        {
            // arrange
            _mockClient.Setup(x => x.GetAsync(string.Empty, new CancellationToken())).ThrowsAsync(new Exception("GET error"));

            // act
            _viewModel.SendCommand.Execute();

            // assert
            _mockLogger.Verify(x => x.Error("GET送信に失敗しました。", It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public void ファイルアップロードボタン押下時_POST送信を実行_受信データを通信ログに出力する()
        {
            // arrange
            var expectedMessage = "Post response";
            _mockClient.Setup(x => x.PostAsync("UploadFile", new CancellationToken())).ReturnsAsync(expectedMessage);

            // act
            _viewModel.PostCommand.Execute();

            // assert
            _mockClient.Verify(x => x.PostAsync("UploadFile", new CancellationToken()), Times.Once);
            _mockLogger.Verify(x => x.Info($"受信データ:\r\n{expectedMessage}"), Times.Once);
        }


        [Fact]
        public void 認証方法を切り替える_ユーザーとパスワードの活性状態も切り替わる()
        {
            // arrange & act
            // 認証方法をAnonymousに設定
            _viewModel.AuthenticationMethod.Value = MainWindowViewModel.AuthenticationMethodType.Anonymous;

            // ユーザー名とパスワードの入力欄が無効化されていることを確認
            Assert.False(_viewModel.IsUserEnabled.Value);
            Assert.False(_viewModel.IsPasswordEnabled.Value);

            // arrange & act
            // 認証方法をBasicに切り替え
            _viewModel.AuthenticationMethod.Value = MainWindowViewModel.AuthenticationMethodType.Basic;

            // ユーザー名とパスワードの入力欄が有効化されていることを確認s
            Assert.True(_viewModel.IsUserEnabled.Value);
            Assert.True(_viewModel.IsPasswordEnabled.Value);
        }

        [Fact]
        public void 画面上の設定情報と保存中の設定情報に差がある_ボタンのうちSaveボタンのみ活性状態にする()
        {
            // arrange
            var config = new ConfigData();
            _mockConfigMapper.Setup(x => x.CreateFrom(_viewModel)).Returns(config);
            _mockConfigService.Setup(x => x.ExistsConfigDifference(config)).Returns(true);

            // act
            _viewModel.HostName.Value = "test"; // 変更してUpdateEnabledを実行させる

            // assert
            Assert.True(_viewModel.SaveCommandEnabled.Value);
            Assert.False(_viewModel.SendCommandEnabled.Value);
            Assert.False(_viewModel.PostCommandEnabled.Value);
        }


        [Fact]
        public void ログファイルが更新される_画面上のログエリアも同じ内容で更新される()
        {
            // arrange
            var newContent = "New log content";

            // act
            _mockLogFileWatcher.Raise(x => x.FileChanged += null, null, newContent);

            // assert
            Assert.Equal(newContent, _viewModel.LogText.Value);
        }
    }
}
