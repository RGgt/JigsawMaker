using JigsawMakerApi.Entities;

namespace JigsawMakerApi.Authorization;

public interface IJwtUtils
{
    public string GenerateToken(User user);
    public TokenValidationResult? ValidateToken(string? token);
}