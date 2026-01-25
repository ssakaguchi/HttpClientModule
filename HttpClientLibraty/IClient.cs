namespace HttpClientService
{
    public interface IClient
    {
        Task<string> GetAsync(string command, CancellationToken cancellationToken = default);

        //string Post(string command);
        Task<string> PostAsync(string command, CancellationToken cancellationToken = default);
    }
}
