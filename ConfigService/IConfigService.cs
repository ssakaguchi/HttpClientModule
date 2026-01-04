namespace ConfigService
{
    public interface IConfigService
    {
        /// <summary> 設定の読み込み </summary>
        public ConfigData Load();

        /// <summary> 設定の保存 </summary>
        public void Save(ConfigData configData);

        /// <summary> 設定に差分があるかどうか </summary>
        public bool ExistsConfigDifference(ConfigData configData);
    }
}
