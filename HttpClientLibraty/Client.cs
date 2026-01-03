using System.Net.Http.Headers;
using System.Text;

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

            if (config.UseBasicAuth)
            {
                // Basic認証ヘッダ付与
                request.Headers.Authorization = CreateBasicAuthHeader(
                    config.User,
                    config.Password
                );
            }

            try
            {
                var httpResponseMessage  = _httpClient.SendAsync(request).GetAwaiter().GetResult();

                // ステータスコードが成功でない場合は例外をスロー
                httpResponseMessage.EnsureSuccessStatusCode();

                return httpResponseMessage;
            }
            catch (HttpRequestException)
            {
                // 通信エラー
                throw;
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

        private static AuthenticationHeaderValue CreateBasicAuthHeader(string user, string password)
        {
            var raw = $"{user}:{password}";
            var bytes = Encoding.UTF8.GetBytes(raw);
            var base64 = Convert.ToBase64String(bytes);
            return new AuthenticationHeaderValue("Basic", base64);
        }
    }
}
