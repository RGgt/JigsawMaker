using System.ComponentModel.DataAnnotations;

namespace JigsawMakerApi.Authorization;

public class AuthenticateRequest
{
    [Required]
    public string? Username { get; set; }
    [Required]
    public string? Password { get; set; }
}
