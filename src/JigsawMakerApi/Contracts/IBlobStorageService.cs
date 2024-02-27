namespace JigsawMakerApi.Contracts;

public interface IBlobStorageService
{
    Task<MemoryStream?> ReadStaticFile(string fileName, CancellationToken cancellationToken);
}