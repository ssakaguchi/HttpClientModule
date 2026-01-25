namespace HttpClientService
{
    public interface IClient
    {
        Task<string> GetAsync(string command, CancellationToken cancellationToken = default);

        Task<string> PostAsync(string command, CancellationToken cancellationToken = default);
    }
}
