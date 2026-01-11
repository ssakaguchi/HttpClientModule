using System.Net.Http.Headers;
using System.Text;
using ConfigService;
using LoggerService;

namespace HttpClientService
{
    public class Client : IClient
    {
        /// <summary> 設定中のベースアドレス </summary>
        private Uri? _currentBaseUri;

        /// <summary>
        /// 設定中のタイムアウト時間（秒）
        /// </summary>
        private int _currentTimeoutSeconds;

        /// <summary> HttpClientインスタンス </summary>
        private HttpClient _httpClient = new();

        private readonly IConfigService _configService;
        private readonly ILog4netAdapter _logger;

        public Client(IConfigService configService, ILog4netAdapter logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public string GetMessage(string command)
        {
            var httpResponseMessage = Get(command);
            return httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        private HttpResponseMessage Get(string command)
        {
            var config = _configService.Load();

            // Httpクライアントの設定
            EnsureHttpClient(config);

            using var request = new HttpRequestMessage(HttpMethod.Get, command);

            _logger.Info($"GET送信します");


            if (config.AuthenticationMethod.Equals("Basic"))
            {
                // Basic認証ヘッダ付与
                request.Headers.Authorization = CreateBasicAuthHeader(
                    config.User,
                    config.Password
                );

                _logger.Info($"  認証方法：Basic認証");
                _logger.Info($"  アカウント認証 ");
                _logger.Info($"    ユーザー名：{config.User}");
                _logger.Info($"    パスワード：{config.Password}");
            }
            else
            {
                _logger.Info($"  認証方法：なし");
            }

            try
            {
                _logger.Info($"  URI：{request.RequestUri}");

                var httpResponseMessage = _httpClient.SendAsync(request).GetAwaiter().GetResult();

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
            UriBuilder uriBuilder = new()
            {
                Scheme = "http",
                Host = config.Host,
                Port = Convert.ToInt32(config.Port),
                Path = config.Path,
            };

            var timeoutSeconds = config.TimeoutSeconds;
            if (_currentBaseUri != null &&
                _currentBaseUri == uriBuilder.Uri &&
                _currentTimeoutSeconds == timeoutSeconds)
            {
                return;
            }

            // HttpClientの初期化処理
            _httpClient.Dispose();
            _httpClient = new HttpClient
            {
                BaseAddress = uriBuilder.Uri,
                Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
            };

            _currentBaseUri = uriBuilder.Uri;
            _currentTimeoutSeconds = timeoutSeconds;
        }

        /// <summary> Basic認証ヘッダの作成 </summary>
        private static AuthenticationHeaderValue CreateBasicAuthHeader(string user, string password)
        {
            var raw = $"{user}:{password}";
            var bytes = Encoding.UTF8.GetBytes(raw);
            var base64 = Convert.ToBase64String(bytes);
            return new AuthenticationHeaderValue("Basic", base64);
        }
    }
}
