namespace HttpClientService
{
    public class Client
    {
        /// <summary>
        /// シングルトン
        /// </summary>
        private static Client? _instance;
        public static Client Instance => _instance ??= new Client();

        /// <summary> 設定中のベースアドレス </summary>
        private string? _currentBaseAddress;

        /// <summary>
        /// 設定中のタイムアウト時間（秒）
        /// </summary>
        private int _currentTimeoutSeconds;

        /// <summary> HttpClientインスタンス </summary>
        private HttpClient _httpClient = new();

        private Client() { }

        public string GetMessage(string command)
        {
            var httpResponseMessage = Get(command);
            return httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        private HttpResponseMessage Get(string command)
        {
            var config = ConfigManager.GetConfigData();

            // Httpクライアントの設定
            EnsureHttpClient(config);

            using var request = new HttpRequestMessage(HttpMethod.Get, command);

            try
            {
                return _httpClient.SendAsync(request).GetAwaiter().GetResult();
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

        /// <summary> Httpクライアントの設定 </summary>
        /// <remarks>前回送信時と設定が変わっていない場合は何もしない</remarks>
        private void EnsureHttpClient(ConfigData config)
        {
            var baseAddress = $"http://{config.Host}:{config.Port}/{config.Path.Trim('/')}";

            var timeoutSeconds = config.TimeoutSeconds;
            if (_currentBaseAddress == baseAddress && _currentTimeoutSeconds == timeoutSeconds)
            {
                return;
            }

            // HttpClientの初期化処理
            _httpClient.Dispose();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
            };

            _currentBaseAddress = baseAddress;
            _currentTimeoutSeconds = timeoutSeconds;
        }
    }
}
