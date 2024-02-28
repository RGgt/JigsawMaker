using JigsawMakerApi.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JigsawMakerApi.Authorization;

public class AuthenticationTokenHelper : IAuthenticationTokenHelper
{
    private readonly SigningCredentials _signingCredentials;

    public AuthenticationTokenHelper(string secret)
    {
        // Convert the secret key from string to byte array and create signing credentials
        var key = Encoding.UTF8.GetBytes(secret);
        _signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
    }

    public string GenerateToken(User user)
    {
        // Create a JWT token handler
        var tokenHandler = new JwtSecurityTokenHandler();
        // Generate claims based on user information
        var claims = WriteClaims(user);
        // Create a token descriptor using the claims and signing credentials
        var tokenDescriptor = CreateTokenDescriptor(claims);

        // Create and return the JWT token
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private void AddUserId(List<Claim> claims, User user)
    {
        // Add a claim for the user ID
        claims.Add(new Claim("UserId", user.Id.ToString()));
    }

    private void AddUserRoles(List<Claim> claims, User user)
    {
        // Add claims for user roles if they exist
        if (user.Roles != null && user.Roles.Any())
        {
            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }
    }

    private SecurityTokenDescriptor CreateTokenDescriptor(List<Claim> claims)
    {
        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = _signingCredentials
        };
    }

    private List<Claim> WriteClaims(User user)
    {
        var claims = new List<Claim>();
        AddUserId(claims, user);
        AddUserRoles(claims, user);
        return claims;
    }

    private TokenValidationResult? ValidateToken(string? token)
    {
        if (token == null) return null;

        try
        {
            // Create a JWT token handler and validation parameters
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingCredentials.Key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
            // Validate the token and extract claims
            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "UserId").Value);
            var roles = jwtToken.Claims.First(x => x.Type == "Roles").Value;
            // Create and return the validation result
            return new TokenValidationResult { UserId = userId, Roles = roles.Split(',') };
        }
        catch
        {
            return null;
        }
    }


    public TokenValidationParameters GetTokenValidationParameters()
    {
        // Return pre-configured validation parameters with the signing key
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingCredentials.Key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    }
}

