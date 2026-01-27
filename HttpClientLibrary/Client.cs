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
        private readonly ILoggerService _logger;

        public Client(IConfigService configService, ILoggerService logger)
        {
            _configService = configService;
            _logger = logger;
        }

        /// <summary> GET送信する </summary>
        public async Task<string> GetAsync(string command, CancellationToken cancellationToken = default)
        {
            var config = _configService.Load();
            EnsureHttpClient(config);

            using var request = new HttpRequestMessage(HttpMethod.Get, command);

            _logger.Info("疎通確認（GET）します");
            ApplyAuthentication(config, request);

            try
            {
                _logger.Info($"  URI：{_httpClient.BaseAddress}");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var statusCode = response.StatusCode;

                _logger.Info($"  ステータスコード：{(int)statusCode} ({statusCode})");

                // ステータスコードが成功でない場合は例外をスロー
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.Error("通信エラーが発生しました", ex);
                throw;
            }
            catch (OperationCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.Error("タイムアウトしました", ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error("未知の例外エラーが発生しました", ex);
                throw;
            }
        }

        /// <summary> ファイルをPOST送信する </summary>
        public async Task<string> PostAsync(string command, CancellationToken cancellationToken = default)
        {
            var config = _configService.Load();

            // Httpクライアントの設定
            EnsureHttpClient(config);

            using var request = new HttpRequestMessage(HttpMethod.Post, command);

            if (!File.Exists(config.UploadFilePath))
            {
                throw new FileNotFoundException("アップロードファイルが見つかりません。", config.UploadFilePath);
            }

            _logger.Info($"ファイルをアップロード（POST）します");

            try
            {
                StreamContent fileContent = new(File.OpenRead(config.UploadFilePath));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                request.Content = fileContent;

                ApplyAuthentication(config, request);

                _logger.Info($"  URI：{_httpClient.BaseAddress}");

                var response = await _httpClient.SendAsync(request, cancellationToken);

                var statusCode = response.StatusCode;
                _logger.Info($"  ステータスコード：{(int)statusCode} ({statusCode})");

                // ステータスコードが成功でない場合は例外をスロー
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (HttpRequestException)
            {
                // 通信エラー
                _logger.Error($"  通信エラーが発生しました");
                throw;
            }
            catch (TaskCanceledException)
            {
                // タイムアウト
                _logger.Error($"  タイムアウトしました");
                throw;
            }
            catch (Exception)
            {
                // その他のエラー
                _logger.Error($"  未知の例外エラーが発生しました");
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

            if (_currentBaseUri != null &&
                _currentBaseUri == uriBuilder.Uri &&
                _currentTimeoutSeconds == config.TimeoutSeconds)
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
            _currentTimeoutSeconds = config.TimeoutSeconds;
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
