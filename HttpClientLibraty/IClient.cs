namespace HttpClientService
{
    public interface IClient
    {
        string GetMessage(string command);

        string Post(string command);
    }
}
