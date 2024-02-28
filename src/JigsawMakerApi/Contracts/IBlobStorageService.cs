namespace JigsawMakerApi.Contracts;

public interface IBlobStorageService
{
    Task<MemoryStream?> ReadStaticFile(string fileName, CancellationToken cancellationToken);
    Task WriteStaticFile(IFormFile file, string fileName, CancellationToken cancellationToken);
    Task DeleteStaticFile(string fileName, CancellationToken cancellationToken);
}