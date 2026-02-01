using System;
using System.Threading.Tasks;
using ConfigService;
using HttpClientService;
using HttpClientWPF.ConfigMapper;
using HttpClientWPF.FileDialogService;
using LoggerService;
using Moq;
using Xunit;

namespace HttpClientWPF.Tests
{
    public class MainWindowViewModelTests : IDisposable
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
        public void Constructor_InitializesPropertiesWithDefaultValues()
        {
            Assert.False(_viewModel.UseHttps.Value);
            Assert.Equal(string.Empty, _viewModel.HostName.Value);
            Assert.Equal(0, _viewModel.PortNo.Value);
            Assert.Equal(string.Empty, _viewModel.Path.Value);
            Assert.Equal(string.Empty, _viewModel.Query.Value);
            Assert.Equal(0, _viewModel.TimeoutSeconds.Value);
            Assert.Equal(MainWindowViewModel.AuthenticationMethodType.Basic, _viewModel.AuthenticationMethod.Value);
            Assert.True(_viewModel.IsUserEnabled.Value);
            Assert.True(_viewModel.IsPasswordEnabled.Value);
        }

        [Fact]
        public void OnLoaded_LoadsConfigurationAndUpdatesViewModel()
        {
            var expectedConfig = new ConfigData();
            var expectedLogContent = "Log content";

            _mockConfigService.Setup(x => x.Load()).Returns(expectedConfig);
            _mockLogFileWatcher.Setup(x => x.ReadLogFileContentAsync()).ReturnsAsync(expectedLogContent);

            _viewModel.LoadedCommand.Execute();

            _mockConfigService.Verify(x => x.Load(), Times.Once);
            _mockConfigMapper.Verify(x => x.ApplyTo(_viewModel, expectedConfig), Times.Once);
            Assert.Equal(expectedLogContent, _viewModel.LogText.Value);
        }

        [Fact]
        public void OnLoaded_HandlesExceptionAndLogsError()
        {
            _mockConfigService.Setup(x => x.Load()).Throws(new Exception("Load error"));

            _viewModel.LoadedCommand.Execute();

            _mockLogger.Verify(x => x.Error("Loadに失敗しました。", It.IsAny<Exception>()), Times.Once);
            Assert.Equal("Loadに失敗しました。", _viewModel.StatusMessage.Value);
        }

        [Fact]
        public void OnUploadFileSelectButtonClicked_UpdatesUploadFilePath()
        {
            var expectedFilePath = "C:\\test\\file.txt";
            _mockOpenFileDialogService.Setup(x => x.OpenFileDialog()).Returns(true);
            _mockOpenFileDialogService.Setup(x => x.FilePath).Returns(expectedFilePath);

            _viewModel.UploadFileSelectCommand.Execute();

            Assert.Equal(expectedFilePath, _viewModel.UploadFilePath.Value);
            Assert.Equal("アップロードファイルの選択", _mockOpenFileDialogService.Object.Title);
        }

        [Fact]
        public void OnUploadFileSelectButtonClicked_HandlesUserCancellation()
        {
            _mockOpenFileDialogService.Setup(x => x.OpenFileDialog()).Returns(false);

            _viewModel.UploadFileSelectCommand.Execute();

            Assert.Equal(string.Empty, _viewModel.UploadFilePath.Value);
        }

        [Fact]
        public void OnSaveButtonClicked_SavesConfigurationSuccessfully()
        {
            var config = new ConfigData();
            _mockConfigMapper.Setup(x => x.CreateFrom(_viewModel)).Returns(config);

            _viewModel.SaveCommand.Execute();

            _mockConfigMapper.Verify(x => x.CreateFrom(_viewModel), Times.Once);
            _mockConfigService.Verify(x => x.Save(config), Times.Once);
            Assert.Equal("設定を保存しました。", _viewModel.StatusMessage.Value);
        }

        [Fact]
        public void OnSaveButtonClicked_HandlesExceptionAndLogsError()
        {
            _mockConfigMapper.Setup(x => x.CreateFrom(_viewModel)).Throws(new Exception("Save error"));

            _viewModel.SaveCommand.Execute();

            _mockLogger.Verify(x => x.Error("設定の保存に失敗しました。", It.IsAny<Exception>()), Times.Once);
            Assert.Equal("設定の保存に失敗しました。", _viewModel.StatusMessage.Value);
        }

        [Fact]
        public async Task OnSendButtonClicked_SendsGetRequestSuccessfully()
        {
            var expectedMessage = "Response data";
            _mockClient.Setup(x => x.GetAsync(string.Empty)).ReturnsAsync(expectedMessage);

            await _viewModel.SendCommand.Execute();
            await Task.Delay(100); // Wait for async operation

            _mockClient.Verify(x => x.GetAsync(string.Empty), Times.Once);
            _mockLogger.Verify(x => x.Info($"受信データ:\r\n{expectedMessage}"), Times.Once);
        }

        [Fact]
        public async Task OnSendButtonClicked_HandlesExceptionAndLogsError()
        {
            _mockClient.Setup(x => x.GetAsync(string.Empty)).ThrowsAsync(new Exception("GET error"));

            await _viewModel.SendCommand.Execute();
            await Task.Delay(100); // Wait for async operation

            _mockLogger.Verify(x => x.Error("GET送信に失敗しました。", It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public async Task OnPostButtonClicked_SendsPostRequestSuccessfully()
        {
            var expectedMessage = "Post response";
            _mockClient.Setup(x => x.PostAsync("UploadFile")).ReturnsAsync(expectedMessage);

            await _viewModel.PostCommand.Execute();
            await Task.Delay(100); // Wait for async operation

            _mockClient.Verify(x => x.PostAsync("UploadFile"), Times.Once);
            _mockLogger.Verify(x => x.Info($"受信データ:\r\n{expectedMessage}"), Times.Once);
        }

        [Fact]
        public void AuthenticationMethod_UpdatesEnabledStates()
        {
            _viewModel.AuthenticationMethod.Value = MainWindowViewModel.AuthenticationMethodType.Anonymous;

            Assert.False(_viewModel.IsUserEnabled.Value);
            Assert.False(_viewModel.IsPasswordEnabled.Value);

            _viewModel.AuthenticationMethod.Value = MainWindowViewModel.AuthenticationMethodType.Basic;

            Assert.True(_viewModel.IsUserEnabled.Value);
            Assert.True(_viewModel.IsPasswordEnabled.Value);
        }

        [Fact]
        public void UpdateEnabled_EnablesSaveCommandWhenConfigDiffers()
        {
            var config = new ConfigData();
            _mockConfigMapper.Setup(x => x.CreateFrom(_viewModel)).Returns(config);
            _mockConfigService.Setup(x => x.ExistsConfigDifference(config)).Returns(true);

            _viewModel.HostName.Value = "test"; // Trigger update

            Assert.True(_viewModel.SaveCommandEnabled.Value);
            Assert.False(_viewModel.SendCommandEnabled.Value);
            Assert.False(_viewModel.PostCommandEnabled.Value);
        }

        [Fact]
        public void ClearMessageCommand_ClearsStatusMessage()
        {
            _viewModel.StatusMessage.Value = "Test message";

            _viewModel.ClearMessageCommand.Execute();

            Assert.Equal(string.Empty, _viewModel.StatusMessage.Value);
        }

        [Fact]
        public void OnLogFileChanged_UpdatesLogText()
        {
            var newContent = "New log content";

            _mockLogFileWatcher.Raise(x => x.FileChanged += null, null, newContent);

            Assert.Equal(newContent, _viewModel.LogText.Value);
        }

        public void Dispose()
        {
            _viewModel?.Dispose();
        }
    }
}