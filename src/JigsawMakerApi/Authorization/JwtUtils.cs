using JigsawMakerApi.Configuration;
using JigsawMakerApi.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JigsawMakerApi.Authorization;

public class JwtUtils : IJwtUtils
{
    private readonly AppSettings _settings;

    public JwtUtils(IOptions<AppSettings> settings)
    {
        _settings = settings.Value;
        if (string.IsNullOrEmpty(_settings.Secret))
            throw new Exception("JWT secret is not configured!");
    }
    public string GenerateToken(User user)
    {
        // Create a JWT token handler
        var tokenHandler = new JwtSecurityTokenHandler();
        // Convert the secret key from string to byte array
        var key = Encoding.UTF8.GetBytes(_settings.Secret!);
        // Define the claims to be included in the token
        var claims = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString())
        };
        // Include user roles in the claims if they exist
        if (user.Roles != null && user.Roles.Any())
        {
            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }
        // Define the security token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        // Create and return the JWT token
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public TokenValidationResult? ValidateToken(string? token)
    {
        if (token == null) return null;
        // Convert the secret key from string to byte array
        var key = Encoding.UTF8.GetBytes(_settings.Secret!);
        try
        {
            // Create a JWT token handler
            var tokenHandler = new JwtSecurityTokenHandler();
            // Define the validation parameters
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
            // Validate the token and extract claims
            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;
            // Extract user ID and roles from the claims
            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "UserId").Value);
            var roles = jwtToken.Claims.First(x => x.Type == "Roles").Value;
            // Create and return the validation result
            var result = new TokenValidationResult { UserId = userId, Roles = roles.Split(',') };
            return result;
        }
        catch
        {
            return null;
        }
    }
}

