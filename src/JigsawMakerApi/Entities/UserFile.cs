using System.ComponentModel.DataAnnotations;

namespace JigsawMakerApi.Entities;

public class UserFile
{
    [Key]
    public int Id { get; set; }
    public string OriginalName{ get; set; }
    public string StoreName { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeleteDate { get; set; }
    public bool IsUploaded { get; set; } = false;
    public DateTime UploadStartDate { get; set; } 
}
