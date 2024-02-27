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
        string connectionStringTemplate = _options.ConnectionStringTemplate;
        string key = await _keyVaultService.GetSecretAsync("StorageAccessKey");
        return $"{connectionStringTemplate};AccountKey={key}";
    }
}
