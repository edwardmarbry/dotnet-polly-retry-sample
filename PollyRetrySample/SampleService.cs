namespace PollyRetrySample;

public interface ISampleService
{
    Task<bool> DoSomething();
}

public class SampleService : ISampleService
{
    private readonly HttpClient _httpClient;

    public SampleService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> DoSomething()
    {
        var response = await _httpClient.GetAsync(new Uri("https://localhost:5001/api/warmup")); // This will retry base on the policy set in program.cs - and retry internally with the message handler. line 19 is not called multiple times.

        return response.IsSuccessStatusCode;
    }
}