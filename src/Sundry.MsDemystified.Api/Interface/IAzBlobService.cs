namespace Sundry.MsDemystified.Api.Interface;
using Azure.Storage.Blobs;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;

public interface IAzBlobService
{
    Task<bool> DownloadFileFromAzBlobNotOptimizedAsync();
    Task<bool> DownloadFileFromAzBlobOptimizedAsync();
}
public class AzBlobService : IAzBlobService
{

    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzBlobSettingsOption _azBlobSettingsOption;

    public AzBlobService(IAzureClientFactory<BlobServiceClient> blobServiceClientFactory,AzBlobSettingsOption azBlobSettingsOption)
    {
        _azBlobSettingsOption = azBlobSettingsOption;
        _blobServiceClient = blobServiceClientFactory.CreateClient(_azBlobSettingsOption.ConnectionName );
    }
    public async Task<bool> DownloadFileFromAzBlobNotOptimizedAsync()
    {
        var container = _blobServiceClient.GetBlobContainerClient(_azBlobSettingsOption.ContainerName);
        var blobs = container.GetBlobs();
        var semaphore = new SemaphoreSlim(10);
        var tasks = new List<Task>();

        foreach (var blob in blobs)
        {
            await semaphore.WaitAsync();

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var blobClient = container.GetBlobClient(blob.Name);
                    using var stream = new MemoryStream();
                    await blobClient.DownloadToAsync(stream);
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }
        await Task.WhenAll(tasks);
        return true;
    }
    private static readonly RecyclableMemoryStreamManager manager = new();

    public async Task<bool> DownloadFileFromAzBlobOptimizedAsync()
    {
        var container = _blobServiceClient.GetBlobContainerClient(_azBlobSettingsOption.ContainerName);
        var blobs = container.GetBlobs();
        var semaphore = new SemaphoreSlim(10);
        var tasks = new List<Task>();

        foreach (var blob in blobs)
        {
            await semaphore.WaitAsync();

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var blobClient = container.GetBlobClient(blob.Name);
                    using var stream = manager.GetStream();
                    await blobClient.DownloadToAsync(stream);
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }
        await Task.WhenAll(tasks);
        return true;
    }
}