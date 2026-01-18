using ConfigService;

namespace HttpClientWPF.ConfigMapper
{
    public sealed class ConfigMapper : IConfigMapper
    {
        public void ApplyTo(MainWindowViewModel vm, ConfigData config)
        {
            if (vm == null) { throw new ArgumentNullException(nameof(vm)); }
            ArgumentNullException.ThrowIfNull(config);

            vm.UseHttps.Value = string.Equals(config.Scheme, "https", StringComparison.OrdinalIgnoreCase);
            vm.HostName.Value = config.Host ?? string.Empty;
            vm.PortNo.Value = int.Parse(config.Port);
            vm.Path.Value = config.Path ?? string.Empty;
            vm.Query.Value = config.Query ?? string.Empty;
            vm.TimeoutSeconds.Value = config.TimeoutSeconds;

            if (Enum.TryParse<MainWindowViewModel.AuthenticationMethodType>(
                    config.AuthenticationMethod,
                    ignoreCase: true,
                    out var method))
            {
                vm.AuthenticationMethod.Value = method;
            }
            else
            {
                vm.AuthenticationMethod.Value = MainWindowViewModel.AuthenticationMethodType.Basic;
            }

            vm.User.Value = config.User ?? string.Empty;
            vm.Password.Value = config.Password ?? string.Empty;
            vm.UploadFilePath.Value = config.UploadFilePath ?? string.Empty;
        }

        public ConfigData CreateFrom(MainWindowViewModel vm)
        {
            if (vm == null) { throw new ArgumentNullException(nameof(vm)); }

            return new ConfigData
            {
                Scheme = vm.UseHttps.Value ? "https" : "http",
                Host = vm.HostName.Value ?? string.Empty,
                Port = vm.PortNo.Value.ToString(),
                Path = vm.Path.Value ?? string.Empty,
                Query = vm.Query.Value ?? string.Empty,
                TimeoutSeconds = vm.TimeoutSeconds.Value,
                AuthenticationMethod = vm.AuthenticationMethod.Value.ToString(),
                User = vm.User.Value ?? string.Empty,
                Password = vm.Password.Value ?? string.Empty,
                UploadFilePath = vm.UploadFilePath.Value ?? string.Empty,
            };
        }
    }
}
