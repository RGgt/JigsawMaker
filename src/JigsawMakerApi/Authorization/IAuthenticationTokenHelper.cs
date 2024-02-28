using JigsawMakerApi.Entities;
using Microsoft.IdentityModel.Tokens;

namespace JigsawMakerApi.Authorization;

public interface IAuthenticationTokenHelper
{
    string GenerateToken(User user);
    TokenValidationParameters GetTokenValidationParameters();

}
