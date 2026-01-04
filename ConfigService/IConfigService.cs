namespace ConfigService
{
    public interface IConfigService
    {
        public ConfigData GetConfigData();

        public void SaveConfigData(ConfigData configData);
    }
}
