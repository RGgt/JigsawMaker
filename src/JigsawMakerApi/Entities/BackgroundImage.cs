using System.ComponentModel.DataAnnotations;

namespace JigsawMakerApi.Entities;

public class BackgroundImage
{
    [Key] 
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string FileName { get; set; }
}
