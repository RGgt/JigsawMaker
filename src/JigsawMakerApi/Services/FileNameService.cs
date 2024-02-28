using JigsawMakerApi.Contracts;

namespace JigsawMakerApi.Services;

public class FileNameService : IFileNameService
{
    public string GenertateNew(string originalName)
    {
        var extension = Path.GetExtension(originalName);
        var name = Path.GetFileNameWithoutExtension(originalName);
        return $"{Guid.NewGuid()}{extension}";
    }
}
