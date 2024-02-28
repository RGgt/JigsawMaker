using System.ComponentModel.DataAnnotations;

namespace JigsawMakerApi.Entities;
public class User
{
    [Key]
    public int Id { get; set; }
    public string[]? Roles { get; set; }
    public string? PublicName { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}