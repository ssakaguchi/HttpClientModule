using Newtonsoft.Json;

namespace HttpClientLibraty
{
    public class Client
    {
        /// <summary>
        /// シングルトン
        /// </summary>
        private static Client? _instance;
        public static Client Instance => _instance ??= new Client();

        /// <summary>
        /// HttpClientインスタンス
        /// </summary>
        private HttpClient _httpClient = new();

        private Client()
        {
        }

        public void Initialize()
        {
            var config = ConfigManager.GetConfigData();
            var baseAddress = $"http://{config.Host}:{config.Port}/{config.Path}";

            // HttpClientの初期化処理
            _httpClient.BaseAddress = new Uri(baseAddress);
            _httpClient.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        }

        public string GetMessage(string command)
        {
            var httpResponseMessage = Get(command);
            return httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        private HttpResponseMessage Get(string command)
        {
            try
            {
                return _httpClient.GetAsync(command).GetAwaiter().GetResult();
            }
            catch (TaskCanceledException)
            {
                // タイムアウト
                throw;
            }
            catch (Exception)
            {
                // その他のエラー
                throw;
            }
        }
    }
}
