using JigsawMakerApi.Authorization;
using JigsawMakerApi.Entities;

namespace JigsawMakerApi.Contracts;
public interface IUserService
{
    AuthenticateResponse? Authenticate(AuthenticateRequest request);
    User? GetById(int? id);
}
