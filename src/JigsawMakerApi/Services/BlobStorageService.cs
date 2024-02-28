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
        // 1. Get the Azure Storage connection string
        string connectionString = await _connectionStringProvider.GetAzureStorageConnectionString();
        // 2. Retrieve container name from configuration
        string containerName = _options.ContainerName;

        // 3. Create a BlobServiceClient using the connection string
        BlobServiceClient blobServiceClient = new (connectionString);

        // 4. Get a reference to the blob container
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        // 5. Get a reference to the specific blob
        BlobClient blobClient = containerClient.GetBlobClient(fileName);
        // 6. Check if the blob exists before downloading
        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return null;
        }
        // 7. Download the blob content to a memory stream
        var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream, cancellationToken);
        // 8. Reset the stream position for reading
        memoryStream.Seek(0, SeekOrigin.Begin);
        // 9.Return the memory stream containing the downloaded file
        return memoryStream;
    }
 
    public async Task WriteStaticFile(IFormFile file, string fileName, CancellationToken cancellationToken) {
        // 1. Get the Azure Storage connection string
        string connectionString = await _connectionStringProvider.GetAzureStorageConnectionString();
        // 2. Retrieve container name from configuration
        string containerName = _options.ContainerName;
        // 3. Create a BlobServiceClient using the connection string
        BlobServiceClient blobServiceClient = new(connectionString);
        // 4. Get a reference to the blob container
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        // 5. Get a reference to the specific blob
        BlobClient blobClient = containerClient.GetBlobClient(fileName);
        // 6. Open the file stream for reading
        using (var stream = file.OpenReadStream())
        {
            // 7. Upload the file content to the blob
            await blobClient.UploadAsync(stream,cancellationToken);
        }
    }
    public async Task DeleteStaticFile(string fileName, CancellationToken cancellationToken)
    {
        // 1. Get the Azure Storage connection string
        string connectionString = await _connectionStringProvider.GetAzureStorageConnectionString();
        // 2. Retrieve container name from configuration
        string containerName = _options.ContainerName;
        // 3. Create a BlobServiceClient using the connection string
        BlobServiceClient blobServiceClient = new(connectionString);
        // 4. Get a reference to the blob container
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        // 5. Get a reference to the specific blob
        BlobClient blobClient = containerClient.GetBlobClient(fileName);
        // 6. Check if the blob exists before deleting
        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return;
        }
        // 7. Delete the blob
        await blobClient.DeleteAsync();
    }

}
