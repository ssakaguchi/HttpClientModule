using Newtonsoft.Json;

namespace ConfigService
{
    public class ConfigManager : IConfigService
    {
        private ConfigData _configData = new();
        private readonly string _filePath;

        public ConfigManager(string filePath) => _filePath = filePath;

        /// <summary> 設定の読み込み </summary>
        public ConfigData Load()
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException("設定ファイルが見つかりません。", _filePath);
            }

            string jsonText = File.ReadAllText(_filePath);
            _configData = JsonConvert.DeserializeObject<ConfigData>(jsonText) ?? new ConfigData();

            return _configData;
        }

        /// <summary> 設定の保存 </summary>
        public void Save(ConfigData configData)
        {
            string jsonText = JsonConvert.SerializeObject(configData, Formatting.Indented);
            File.WriteAllText(_filePath, jsonText);
            _configData = configData;
        }

        /// <summary> 設定に差分があるかどうか </summary>
        public bool ExistsConfigDifference(ConfigData configData) => !_configData.Equals(configData);
    }

    public class ConfigData : IEquatable<ConfigData>
    {
        [JsonProperty("scheme")]
        public string Scheme { get; set; } = "http";

        [JsonProperty("host_name")]
        public string Host { get; set; } = string.Empty;

        [JsonProperty("port_no")]
        public string Port { get; set; } = string.Empty;

        [JsonProperty("path")]
        public string Path { get; set; } = string.Empty;

        [JsonProperty("timeout_seconds")]
        public int TimeoutSeconds { get; set; } = 20;

        [JsonProperty("authentication_method")]
        public string AuthenticationMethod { get; set; } = string.Empty;

        [JsonProperty("user")]
        public string User { get; set; } = string.Empty;

        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;

        /// <summary> 等値比較 </summary>
        public bool Equals(ConfigData? other)
        {
            if (other is null) return false;

            return Scheme == other.Scheme &&
                    Host == other.Host &&
                    Port == other.Port &&
                    Path == other.Path &&
                    TimeoutSeconds == other.TimeoutSeconds &&
                    AuthenticationMethod == other.AuthenticationMethod &&
                    User == other.User &&
                    Password == other.Password;
        }

        /// <summary> 等値比較 </summary>
        public override bool Equals(object? obj)
            => Equals(obj as ConfigData);

        /// <summary> ハッシュコード取得 </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Scheme,
                Host,
                Port,
                Path,
                TimeoutSeconds,
                AuthenticationMethod,
                User,
                Password
            );
        }

        /// <summary> 等値比較演算子 </summary>
        public static bool operator ==(ConfigData left, ConfigData right)
            => Equals(left, right);

        /// <summary> 非等値比較演算子 </summary>
        public static bool operator !=(ConfigData left, ConfigData right)
            => !Equals(left, right);
    }

}
