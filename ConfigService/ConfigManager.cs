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
}
