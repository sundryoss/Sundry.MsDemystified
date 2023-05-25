using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Mvc.Testing;
using BenchmarkDotNet.Running;

internal class ProgramX
{
    private static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<BenchmarkAPIPerformance>();
    }
}

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class BenchmarkAPIPerformance
{
    private static HttpClient _httpClient;

    [Params(25,50)]
    public int N;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var factory = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(_ =>{});
        _httpClient = factory.CreateClient();
    }

    [Benchmark]
    public async Task DownloadOptimized()
    {
        for (int i = 0; i < N; i++)
        {
            var response = await _httpClient.GetAsync("/DownloadOptimized");
        }
    }

    [Benchmark]
    public async Task DownloadNotOptimized()
    {
        for (int i = 0; i < N; i++)
        {
            var response = await _httpClient.GetAsync("/DownloadNotOptimized");
        }
    }
}
