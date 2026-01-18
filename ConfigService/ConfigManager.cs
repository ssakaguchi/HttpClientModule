using Newtonsoft.Json;

namespace ConfigService
{
    public class ConfigManager : IConfigService
    {
        private ConfigData _config = new();
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
            _config = JsonConvert.DeserializeObject<ConfigData>(jsonText) ?? new ConfigData();

            return _config;
        }

        /// <summary> 設定の保存 </summary>
        public void Save(ConfigData config)
        {
            string jsonText = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(_filePath, jsonText);
            _config = config;
        }

        /// <summary> 設定に差分があるかどうか </summary>
        public bool ExistsConfigDifference(ConfigData configData) => !_config.Equals(configData);
    }
}
