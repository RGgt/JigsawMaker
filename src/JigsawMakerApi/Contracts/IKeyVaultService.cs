namespace JigsawMakerApi.Contracts;

public interface IKeyVaultService
{
    Task<string> GetSecretAsync(string secretName);
}