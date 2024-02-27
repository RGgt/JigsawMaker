namespace JigsawMakerApi.Contracts;
public interface IConnectionStringBuilderService
{
    Task<string> GetAzureStorageConnectionString();
}