using JigsawMakerApi.Configuration;
using JigsawMakerApi.Contracts;
using Microsoft.Extensions.Options;

namespace JigsawMakerApi.Services;

public class ConnectionStringBuilderService : IConnectionStringBuilderService
{
    private readonly IKeyVaultService _keyVaultService;
    private readonly AzureBlobStorageOptions _options;

    public ConnectionStringBuilderService(IKeyVaultService keyVaultService, IOptions<AzureBlobStorageOptions> options)
    {
        _keyVaultService = keyVaultService;
        _options = options.Value;
    }

    public async Task<string> GetAzureStorageConnectionString( )
    {
        // 1. Retrieve the connection string template from configuration
        string connectionStringTemplate = _options.ConnectionStringTemplate;
        // 2. Retrieve the storage access key from Key Vault using an asynchronous service
        string key = await _keyVaultService.GetSecretAsync("StorageAccessKey");
        // 3. Format the complete connection string by combining the template and retrieved key
        return $"{connectionStringTemplate};AccountKey={key}";
    }
}
