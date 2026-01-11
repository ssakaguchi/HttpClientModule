using Newtonsoft.Json;

namespace ConfigService
{
    public class ConfigData : IEquatable<ConfigData>
    {
        [JsonProperty("scheme")]
        public string Scheme { get; set; } = "http";

        [JsonProperty("host_name")]
        public string Host { get; set; } = string.Empty;

        [JsonProperty("port_no")]
        public string Port { get; set; } = string.Empty;

        [JsonProperty("path")]
        public string Path { get; set; } = string.Empty;

        [JsonProperty("query")]
        public string Query { get; set; } = string.Empty;

        [JsonProperty("timeout_seconds")]
        public int TimeoutSeconds { get; set; } = 20;

        [JsonProperty("authentication_method")]
        public string AuthenticationMethod { get; set; } = string.Empty;

        [JsonProperty("user")]
        public string User { get; set; } = string.Empty;

        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;

        /// <summary> 等値比較 </summary>
        public bool Equals(ConfigData? other)
        {
            if (other is null) return false;

            return Scheme == other.Scheme &&
                    Host == other.Host &&
                    Port == other.Port &&
                    Path == other.Path &&
                    Query == other.Query &&
                    TimeoutSeconds == other.TimeoutSeconds &&
                    AuthenticationMethod == other.AuthenticationMethod &&
                    User == other.User &&
                    Password == other.Password;
        }

        /// <summary> 等値比較 </summary>
        public override bool Equals(object? obj)
            => Equals(obj as ConfigData);

        /// <summary> ハッシュコード取得 </summary>
        public override int GetHashCode()
        {
            // HashCode.Combine の引数は最大8つまでなので、9つ以上の場合はネストして結合
            return HashCode.Combine(
                HashCode.Combine(
                    Scheme,
                    Host,
                    Port,
                    Path,
                    Query,
                    TimeoutSeconds,
                    AuthenticationMethod,
                    User
                ),
                Password
            );
        }

        /// <summary> 等値比較演算子 </summary>
        public static bool operator ==(ConfigData left, ConfigData right)
            => Equals(left, right);

        /// <summary> 非等値比較演算子 </summary>
        public static bool operator !=(ConfigData left, ConfigData right)
            => !Equals(left, right);
    }
}
