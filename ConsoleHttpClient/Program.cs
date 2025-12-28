using HttpClientLibraty;

public class Programa
{
    static void Main(string[] args)
    {
		try
		{
			var client = Client.Instance;
			client.Initialize();
			var message = client.GetMessage(string.Empty);
		}
		catch (Exception  ex)
		{
            Console.WriteLine(ex.Message);
		}
    }
}

