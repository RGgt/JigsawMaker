using JigsawMakerApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace JigsawMakerApi.DataAccess;

public class AppDbContext: DbContext
{
    public DbSet<BackgroundImage> BackgroundImages { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
