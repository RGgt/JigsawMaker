namespace JigsawMakerApi.Authorization;


public readonly record struct TokenValidationResult(int UserId, string[] Roles);
