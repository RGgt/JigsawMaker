using JigsawMakerApi.Authorization;
using JigsawMakerApi.Contracts;
using JigsawMakerApi.Entities;

namespace JigsawMakerApi.Services;

public class UserService : IUserService
{

    // users hardcoded for simplicity, store in a db with hashed passwords in production applications
    private List<User> _users = new List<User>
    {
        // TODO: Replace with actual user data retrieval from a database
        // TODO: Ensure passwords are securely hashed before storing them
        new User { Id = 1, Username = "test", Password = "test", Roles=new string[] { "member","tester","admin"} },
        new User { Id = 2, Username = "test2", Password = "test2", Roles=new string[] { "member"} }
    };

    private readonly IJwtUtils _jwtUtils;

    public UserService(IJwtUtils jwtUtils)
    {
        _jwtUtils = jwtUtils;
    }
    public AuthenticateResponse? Authenticate(AuthenticateRequest request)
    {
        // Find the user by username
        var user = _users.SingleOrDefault(x => x.Username == request.Username && x.Password == request.Password);
        // return null if user not found
        if (user == null)
            return null;
        // Authentication successful so generate JWT token
        var token = _jwtUtils.GenerateToken(user);
        return new AuthenticateResponse(user, token);
    }

    public User? GetById(int? id)
    {
        if (id == null)
            return null;
        var user = _users.SingleOrDefault(x => x.Id == id);
        // return null if user not found
        if (user == null)
            return null;
        return user;
    }
}

