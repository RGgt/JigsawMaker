using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using JigsawMakerApi.Configuration;
using JigsawMakerApi.Contracts;
using Microsoft.Extensions.Options;

namespace JigsawMakerApi.Services;

public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _client;

    public KeyVaultService(IOptions<AzureKeyVaultOptions> congiguredOptions)
    {
        string azureVaultName = congiguredOptions.Value.VaultName;
        SecretClientOptions options = new()
        {
            Retry =
        {
            Delay= TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(16),
            MaxRetries = 5,
            Mode = RetryMode.Exponential
         }
        };
        _client = new SecretClient(new Uri(azureVaultName), new DefaultAzureCredential(), options);
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        KeyVaultSecret secret = await _client.GetSecretAsync(secretName);
        return secret.Value;
    }
}
