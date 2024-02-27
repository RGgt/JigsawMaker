using Azure.Storage.Blobs;
using JigsawMakerApi.Configuration;
using JigsawMakerApi.Contracts;
using Microsoft.Extensions.Options;

namespace JigsawMakerApi.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly AzureBlobStorageOptions _options;
    private readonly IConnectionStringBuilderService _connectionStringProvider;
    public BlobStorageService(IOptions<AzureBlobStorageOptions> options, IConnectionStringBuilderService connectionStringProvider)
    {
        _options = options.Value;
        _connectionStringProvider = connectionStringProvider;
    }
    public async Task<MemoryStream?> ReadStaticFile(string fileName, CancellationToken cancellationToken)
    {
        string connectionString = await _connectionStringProvider.GetAzureStorageConnectionString();
        string containerName = _options.ContainerName;

        // Set connection string
        BlobServiceClient blobServiceClient = new (connectionString);

        // Get a reference to the container
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        // Get a reference to the specific blob
        BlobClient blobClient = containerClient.GetBlobClient(fileName);
        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return null;
        }

        // Download to memory stream
        var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream, cancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}
