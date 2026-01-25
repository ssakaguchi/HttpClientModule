namespace HttpClientService
{
    public interface IClient
    {
        string Get(string command);

        string Post(string command);
    }
}
