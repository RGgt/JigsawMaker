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
        // Retrieve the Azure Key Vault name from configuration
        string azureVaultName = congiguredOptions.Value.VaultName;
        // Configure retry options for secret client calls
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
        // Create a SecretClient instance
        _client = new SecretClient(
            new Uri(azureVaultName), 
            new DefaultAzureCredential(), 
            options);
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        KeyVaultSecret secret = await _client.GetSecretAsync(secretName);
        return secret.Value;
    }
}
