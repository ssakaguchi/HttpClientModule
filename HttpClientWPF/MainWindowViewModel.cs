using Reactive.Bindings;
using HttpClientLibraty;
using System;

namespace HttpClientWPF
{
    public class MainWindowViewModel : BindableBase
    {
        public ReactiveProperty<string> HostName { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<int> PortNo { get; } = new ReactiveProperty<int>();
        public ReactiveProperty<string> Path { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<int> TimeoutSeconds { get; } = new ReactiveProperty<int>();

        public ReactiveCommand LoadedCommand { get; } = new();
        public ReactiveCommand SaveCommand { get; } = new ReactiveCommand();
        public ReactiveCommand SendCommand { get; } = new ReactiveCommand();

        public MainWindowViewModel()
        {
            SaveCommand.Subscribe(this.OnSaveButtonClicked);
            SendCommand.Subscribe(this.OnSendButtonClicked);
            LoadedCommand.Subscribe(this.OnLoaded);
        }

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
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OnSendButtonClicked()
        {
            try
            {
                var client = Client.Instance;
                client.Initialize();
                var message = client.GetMessage(string.Empty);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
