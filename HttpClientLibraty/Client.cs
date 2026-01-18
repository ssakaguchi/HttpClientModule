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

        /// <summary> コマンドをGET送信する </summary>
        public string GetMessage(string command)
        {
            var httpResponseMessage = Get(command);
            return httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

         /// <summary> ファイルをPOST送信する </summary>
        public string Post(string command, string filePath)
        {
            var config = _configService.Load();

            // Httpクライアントの設定
            EnsureHttpClient(config);

            _logger.Info($"POST送信します");

            using var request = new HttpRequestMessage(HttpMethod.Post, command);

            try
            {
                StreamContent fileContent = new(File.OpenRead(filePath));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                request.Content = fileContent;
            
                ApplyAuthentication(config, request);

                _logger.Info($"  URI：{_httpClient.BaseAddress}");

                var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();

                // ステータスコードが成功でない場合は例外をスロー
                response.EnsureSuccessStatusCode();

                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
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

        /// <summary> GET送信する </summary>
        private HttpResponseMessage Get(string command)
        {
            var config = _configService.Load();

            // Httpクライアントの設定
            EnsureHttpClient(config);

            using var request = new HttpRequestMessage(HttpMethod.Get, command);

            _logger.Info($"GET送信します");

            ApplyAuthentication(config, request);

            try
            {
                _logger.Info($"  URI：{_httpClient.BaseAddress}");

                var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();

                // ステータスコードが成功でない場合は例外をスロー
                response.EnsureSuccessStatusCode();

                return response;
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
                Scheme = config.Scheme,
                Host = config.Host,
                Port = Convert.ToInt32(config.Port),
                Path = config.Path.TrimStart('/').TrimEnd('/') + "/",
                Query = config.Query
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

        /// <summary> 認証情報の適用 </summary>
        private void ApplyAuthentication(ConfigData config, HttpRequestMessage request)
        {
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
        }
    }
}
