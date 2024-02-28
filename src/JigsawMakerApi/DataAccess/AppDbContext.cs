using JigsawMakerApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace JigsawMakerApi.DataAccess;

public class AppDbContext: DbContext
{
    public DbSet<BackgroundImage> BackgroundImages { get; set; }
    public DbSet<Bird> Birds { get; set; }
    public DbSet<UserFile> UserFiles { get; set; }
    public DbSet<User> Users { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
