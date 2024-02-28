using JigsawMakerApi.Entities;

namespace JigsawMakerApi.Authorization;

public class AuthenticateResponse
{
    public int Id { get; set; }
    public string? PublicName { get; set; }
    public string? Username { get; set; }
    public string[]? Roles { get; set; }
    public string Token { get; set; }
    public AuthenticateResponse(User user, string token)
    {
        Id = user.Id;
        Username = user.Username;
        PublicName = user.PublicName;
        Roles = user.Roles;
        Token = token;
    }
}
